using AutoMapper;
using ControleCerto.DTOs.RecurringTransaction;
using ControleCerto.DTOs.Notification;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using ControleCerto.Validations;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using ControleCerto.DTOs.Transaction;
using Microsoft.IdentityModel.Tokens;


namespace ControleCerto.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly ITransactionService _transactionService;

        public RecurringTransactionService(AppDbContext appDbContext, IMapper mapper, INotificationService notificationService, ITransactionService transactionService)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _notificationService = notificationService;
            _transactionService = transactionService;
        }

        public async Task<Result<InfoRecurringTransactionResponse>> CreateReccuringTransactionAsync(CreateRecurringTransactionRequest request, int userId)
        {
            var validationResult = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);

            if (validationResult.IsError)
            {
                return new Result<InfoRecurringTransactionResponse>(validationResult.Error);
            }

            using var dbTransaction = _appDbContext.Database.BeginTransaction();

            try
            {
                var recurringTransactionToCreate = _mapper.Map<RecurringTransaction>(request);
                var recurrencegRuleToCreate = _mapper.Map<RecurrenceRule>(request.RecurrenceRule);

                var createdRecurrenceRule = await _appDbContext.RecurrenceRules.AddAsync(recurrencegRuleToCreate);
                await _appDbContext.SaveChangesAsync();

                recurringTransactionToCreate.RecurrenceRule = createdRecurrenceRule.Entity;
                recurringTransactionToCreate.UserId = userId;

                var createdRecurringTransaction = await _appDbContext.RecurringTransactions.AddAsync(recurringTransactionToCreate);
                await _appDbContext.SaveChangesAsync();

                await dbTransaction.CommitAsync();

                return _mapper.Map<InfoRecurringTransactionResponse>(createdRecurringTransaction.Entity);
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();

                return new AppError(ex.Message, ErrorTypeEnum.InternalError);
            }
            



            
        }

        public async Task<List<InfoRecurringTransactionResponse>> GetRecurringTransactionsAsync(int userId)
        {
            var recurringTransactions = await _appDbContext.RecurringTransactions
                .Include(rt => rt.RecurrenceRule)
                .Include(rt => rt.Instances)
                .Where(rt => rt.UserId == userId)
                .ToListAsync();

            return _mapper.Map<List<InfoRecurringTransactionResponse>>(recurringTransactions);
        }

        public async Task<Result<InfoRecurringTransactionResponse>> UpdateRecurringTransactionAsync(long recurringId, UpdateRecurringTransactionRequest request, int userId)
        {
            var validationResult = RecurringTransactionValidations.ValidateUpdateRecurringTransactionRequest(request);

            if (validationResult.IsError)
            {
                return new Result<InfoRecurringTransactionResponse>(validationResult.Error);
            }

            var recurringTransaction = await _appDbContext.RecurringTransactions
                .Include(rt => rt.RecurrenceRule)
                .FirstOrDefaultAsync(rt => rt.Id == recurringId && rt.UserId == userId);

            if (recurringTransaction is null)
            {
                return new AppError("Transação recorrente não encontrada.", ErrorTypeEnum.NotFound);
            }

            recurringTransaction.Description = request.Description;
            recurringTransaction.Amount = request.Amount;
            recurringTransaction.Destination = request.Destination ?? recurringTransaction.Destination;
            recurringTransaction.JustForRecord = request.JustForRecord;
            recurringTransaction.Type = request.Type;
            recurringTransaction.StartDate = request.StartDate;
            recurringTransaction.EndDate = request.EndDate;
            recurringTransaction.AccountId = request.AccountId;
            recurringTransaction.CategoryId = request.CategoryId;
            recurringTransaction.IsActive = request.IsActive;
            recurringTransaction.UpdatedAt = DateTime.UtcNow;

            var recurrenceRule = recurringTransaction.RecurrenceRule;
            recurrenceRule.Frequency = request.RecurrenceRule.Frequency;
            recurrenceRule.IsEveryDay = request.RecurrenceRule.IsEveryDay;
            recurrenceRule.DaysOfWeek = request.RecurrenceRule.DaysOfWeek;
            recurrenceRule.DayOfWeek = request.RecurrenceRule.DayOfWeek;
            recurrenceRule.DayOfMonth = request.RecurrenceRule.DayOfMonth;
            recurrenceRule.MonthOfYear = request.RecurrenceRule.MonthOfYear;
            recurrenceRule.DayOfMonthForYearly = request.RecurrenceRule.DayOfMonthForYearly;
            recurrenceRule.Interval = request.RecurrenceRule.Interval;

            _appDbContext.RecurringTransactions.Update(recurringTransaction);
            _appDbContext.RecurrenceRules.Update(recurrenceRule);

            await _appDbContext.SaveChangesAsync();

            return _mapper.Map<InfoRecurringTransactionResponse>(recurringTransaction);
        }

        public async Task<Result<bool>> DeleteRecurringTransactionAsync(long recurringId, int userId)
        {
            var recurringTransaction = await _appDbContext.RecurringTransactions
                .Include(rt => rt.RecurrenceRule)
                .FirstOrDefaultAsync(rt => rt.Id == recurringId && rt.UserId == userId);

            if (recurringTransaction is null)
            {
                return new AppError("Transação recorrente não encontrada.", ErrorTypeEnum.NotFound);
            }

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();

            try
            {
                _appDbContext.RecurringTransactions.Remove(recurringTransaction);
                await _appDbContext.SaveChangesAsync();

                var recurrenceRule = recurringTransaction.RecurrenceRule;

                var ruleInUse = await _appDbContext.RecurringTransactions
                    .AnyAsync(rt => rt.RecurrenceRuleId == recurrenceRule.Id);

                if (!ruleInUse)
                {
                    _appDbContext.RecurrenceRules.Remove(recurrenceRule);
                    await _appDbContext.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return new AppError("Erro ao remover transação recorrente.", ErrorTypeEnum.InternalError);
            }
        }

        public async Task<List<InfoRecurringTransactionInstanceResponse>> GetRecurringTransactionInstancesAsync(InstanceStatusEnum status, int userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _appDbContext.RecurringTransactionInstances
                .Include(i => i.RecurringTransaction)
                .Where(i => i.Status == status && i.RecurringTransaction.UserId == userId);

            if (startDate.HasValue)
            {
                query = query.Where(i => i.ProcessedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(i => i.ProcessedDate <= endDate.Value);
            }

            var instanceList = await query.Select(i => _mapper.Map<InfoRecurringTransactionInstanceResponse>(i)).ToListAsync();

            return instanceList;
        }

        public async Task<Result<List<InfoRecurringTransactionInstanceResponse>>> ProcessPendingRecurringTransactionInstances(List<long> pendingTransactions, InstanceStatusEnum action, int userId, string? rejectReason)
        {
            if (pendingTransactions.Count == 0)
            {
                return new AppError("Transa��es n�o encontradas", ErrorTypeEnum.NotFound);
            }

            if (action == InstanceStatusEnum.PENDING || action == InstanceStatusEnum.ERROR)
            {
                return new AppError("Transa��es n�o podem alterar seu status para 'Pendente' ou 'Erro'", ErrorTypeEnum.BusinessRule);
            }

            var today = DateTime.UtcNow;

            var resolvedPendingRecurring = new List<InfoRecurringTransactionInstanceResponse>();

            var recurringIntances = _appDbContext.RecurringTransactionInstances
                .Where(i => i.RecurringTransaction.UserId == userId && pendingTransactions.Contains(i.Id) && !i.ActualTransactionId.HasValue)
                .Include(i => i.RecurringTransaction);

            using var transaction = _appDbContext.Database.BeginTransaction();

            try
            {
                foreach (var recurring in recurringIntances)
                {
                    try
                    {
                        recurring.Status = action;

                        if (action == InstanceStatusEnum.REJECTED && !rejectReason.IsNullOrEmpty())
                        {
                            recurring.RejectionReason = rejectReason;
                        }

                        if (action == InstanceStatusEnum.CONFIRMED)
                        {
                            var transactionToCreate = new CreateTransactionRequest
                            {
                                AccountId = recurring.RecurringTransaction.AccountId,
                                Amount = recurring.RecurringTransaction.Amount,
                                CategoryId = recurring.RecurringTransaction.CategoryId,
                                Description = recurring.RecurringTransaction.Description,
                                Destination = recurring.RecurringTransaction.Destination ?? "",
                                JustForRecord = recurring.RecurringTransaction.JustForRecord,
                                PurchaseDate = today,
                                Type = recurring.RecurringTransaction.Type
                            };

                            var resultTransaction = await _transactionService.CreateTransactionAsync(transactionToCreate, userId);

                            if (resultTransaction.IsError)
                            {
                                recurring.Status = InstanceStatusEnum.ERROR;
                                recurring.RejectionReason = resultTransaction.Error.ErrorMessage;
                            }
                        }

                        resolvedPendingRecurring.Add(_mapper.Map<InfoRecurringTransactionInstanceResponse>(recurring));
                    }
                    catch (Exception ex)
                    {
                        recurring.Status = InstanceStatusEnum.ERROR;
                        recurring.RejectionReason = ex.Message;
                        //_logger.LogError(ex,
                        //    "Erro ao processar recorr�ncia {RecurringTransactionId} para {Date}",
                        //    recurringTransaction.Id,
                        //    today);
                    }
                }

                _appDbContext.UpdateRange(recurringIntances);

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return resolvedPendingRecurring;
            } catch
            {
                await transaction.RollbackAsync();
                return new AppError("Algo ocorreu no processamento das pend�ncias e elas n�o foram conclu�das", ErrorTypeEnum.InternalError);
            }

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
                    // DI�RIAS
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

                    // Enviar notifica��o para o usu�rio
                    await _notificationService.SendUserNotificationAsync(
                        new CreateNotificationRequest
                        {
                            Title = "Nova transa��o recorrente criada!",
                            Message = $"Nova {(recurringTransaction.Type == TransactionTypeEnum.EXPENSE ? "despesa" : "receita")} " +
                            $"recorrente foi gerada com t�tulo {recurringTransaction.Description}. Clique aqui e aprove a pend�ncia.",
                            Type = NotificationTypeEnum.CONFIRMRECURRENCE,
                            ExpiresAt = today.AddDays(15),
                            UserId = recurringTransaction.UserId,
                            //ActionPath = "NOT IMPLEMENTED YET"
                        },
                        recurringTransaction.UserId);

                }
                catch (Exception ex)
                {
                    //_logger.LogError(ex,
                    //    "Erro ao processar recorr�ncia {RecurringTransactionId} para {Date}",
                    //    recurringTransaction.Id,
                    //    today);
                }
            }

            await _appDbContext.SaveChangesAsync();
        }

    }
}