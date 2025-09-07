using AutoMapper;
using ControleCerto.Enums;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;

        public RecurringTransactionService(AppDbContext appDbContext, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public async Task<List<RecurringTransaction>> GetRecurringTransactionsToProcessAsync(DateTime targetDate)
        {
            var dayOfWeek = (int)targetDate.DayOfWeek;
            var dayOfMonth = targetDate.Day;
            var monthOfYear = targetDate.Month;
            var isLastDayOfMonth = targetDate.Date == new DateTime(targetDate.Year, monthOfYear, 1).AddMonths(1).AddDays(-1).Date;

            return await _appDbContext.RecurringTransactions
                .Include(rt => rt.RecurrenceRule)
                .Include(rt => rt.Account)
                .Include(rt => rt.Category)
                .Where(rt => rt.IsActive &&
                            rt.StartDate <= targetDate &&
                            (rt.EndDate == null || rt.EndDate >= targetDate) &&
                            !rt.Instances.Any(i => i.ScheduledDate.Date == targetDate.Date))

                .Where(rt =>
                    // DIÁRIAS
                    (rt.RecurrenceRule.Frequency == RecurrenceFrequencyEnum.DAILY &&
                     (rt.RecurrenceRule.IsEveryDay ||
                      (rt.RecurrenceRule.DaysOfWeek != null &&
                       rt.RecurrenceRule.DaysOfWeek.Contains($"{dayOfWeek}")))) ||

                    // SEMANAIS
                    (rt.RecurrenceRule.Frequency == RecurrenceFrequencyEnum.WEEKLY &&
                     rt.RecurrenceRule.DayOfWeek == dayOfWeek &&
                     ShouldProcessWeekly(rt.RecurrenceRule, rt.StartDate, targetDate)) ||

                    // MENSALES
                    (rt.RecurrenceRule.Frequency == RecurrenceFrequencyEnum.MONTHLY &&
                     ((rt.RecurrenceRule.DayOfMonth == dayOfMonth) ||
                      (rt.RecurrenceRule.DayOfMonth == -1 && isLastDayOfMonth)) &&
                     ShouldProcessMonthly(rt.RecurrenceRule, rt.StartDate, targetDate)) ||

                    // ANUAIS
                    (rt.RecurrenceRule.Frequency == RecurrenceFrequencyEnum.YEARLY &&
                     rt.RecurrenceRule.MonthOfYear == monthOfYear &&
                     rt.RecurrenceRule.DayOfMonthForYearly == dayOfMonth &&
                     ShouldProcessYearly(rt.RecurrenceRule, rt.StartDate, targetDate))
                )
                .ToListAsync();
        }

        private bool ShouldProcessWeekly(RecurrenceRule rule, DateTime startDate, DateTime targetDate)
        {
            if (rule.Interval == 1) return true;

            var weeksSinceStart = (int)((targetDate - startDate).TotalDays / 7);
            return weeksSinceStart % rule.Interval == 0;
        }

        private bool ShouldProcessMonthly(RecurrenceRule rule, DateTime startDate, DateTime targetDate)
        {
            if (rule.Interval == 1) return true;

            var monthsSinceStart = ((targetDate.Year - startDate.Year) * 12) +
                                  (targetDate.Month - startDate.Month);
            return monthsSinceStart % rule.Interval == 0;
        }

        private bool ShouldProcessYearly(RecurrenceRule rule, DateTime startDate, DateTime targetDate)
        {
            if (rule.Interval == 1) return true;

            var yearsSinceStart = targetDate.Year - startDate.Year;
            return yearsSinceStart % rule.Interval == 0;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ProcessRecurringTransactionsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var recurringTransactions = await GetRecurringTransactionsToProcessAsync(today);

            foreach (var recurringTransaction in recurringTransactions)
            {
                try
                {
                    var instance = new RecurringTransactionInstance
                    {
                        RecurringTransactionId = recurringTransaction.Id,
                        ScheduledDate = today,
                        Status = InstanceStatusEnum.PENDING,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _appDbContext.RecurringTransactionInstances.AddAsync(instance);

                    // Enviar notificação para o usuário
                    await _notificationService.NotifyPendingRecurringTransactionAsync(
                        recurringTransaction.UserId,
                        recurringTransaction,
                        instance);
                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex,
                    //    "Erro ao processar recorrência {RecurringTransactionId} para {Date}",
                    //    recurringTransaction.Id,
                    //    today);
                }
            }

            await _appDbContext.SaveChangesAsync();
        }

    }
}