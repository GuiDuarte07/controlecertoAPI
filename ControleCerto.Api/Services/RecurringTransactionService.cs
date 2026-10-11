using AutoMapper;
using ControleCerto.DTOs.RecurringTransaction;
using ControleCerto.DTOs.Transaction;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using ControleCerto.Validations;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace ControleCerto.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ITransactionService _transactionService;

        public RecurringTransactionService(AppDbContext context, IMapper mapper, ITransactionService transactionService)
        {
            _context = context;
            _mapper = mapper;
            _transactionService = transactionService;
        }

        public async Task<Result<InfoRecurringTransactionResponse>> CreateRecurringTransactionAsync(CreateRecurringTransactionRequest request, int userId)
        {
            var validation = await ValidateRequestAsync(request, userId);
            if (validation.IsError) return validation.Error;

            await using var databaseTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var recurringTransaction = _mapper.Map<RecurringTransaction>(request);
                recurringTransaction.UserId = userId;
                recurringTransaction.StartDate = request.StartDate.Date;
                recurringTransaction.EndDate = request.EndDate?.Date;
                recurringTransaction.Description = request.Description.Trim();
                recurringTransaction.Destination = request.Destination?.Trim() ?? string.Empty;
                recurringTransaction.RecurrenceRule = CreateRule(request.RecurrenceRule);

                await _context.RecurringTransactions.AddAsync(recurringTransaction);
                await _context.SaveChangesAsync();
                await databaseTransaction.CommitAsync();
                return _mapper.Map<InfoRecurringTransactionResponse>(recurringTransaction);
            }
            catch (Exception)
            {
                await databaseTransaction.RollbackAsync();
                return new AppError("Não foi possível criar a transação recorrente.", ErrorTypeEnum.InternalError);
            }
        }

        public async Task<List<InfoRecurringTransactionResponse>> GetRecurringTransactionsAsync(int userId)
        {
            var transactions = await _context.RecurringTransactions.AsNoTracking()
                .Include(transaction => transaction.RecurrenceRule)
                .Include(transaction => transaction.Instances)
                .Where(transaction => transaction.UserId == userId)
                .OrderByDescending(transaction => transaction.IsActive)
                .ThenBy(transaction => transaction.Description)
                .ToListAsync();
            return _mapper.Map<List<InfoRecurringTransactionResponse>>(transactions);
        }

        public async Task<Result<InfoRecurringTransactionResponse>> UpdateRecurringTransactionAsync(long recurringId, UpdateRecurringTransactionRequest request, int userId)
        {
            var validation = await ValidateUpdateRequestAsync(request, userId);
            if (validation.IsError) return validation.Error;

            var recurringTransaction = await _context.RecurringTransactions
                .Include(transaction => transaction.RecurrenceRule)
                .Include(transaction => transaction.Instances)
                .FirstOrDefaultAsync(transaction => transaction.Id == recurringId && transaction.UserId == userId);
            if (recurringTransaction is null) return new AppError("Transação recorrente não encontrada.", ErrorTypeEnum.NotFound);

            recurringTransaction.Description = request.Description.Trim();
            recurringTransaction.Amount = request.Amount;
            recurringTransaction.Destination = request.Destination?.Trim() ?? string.Empty;
            recurringTransaction.JustForRecord = request.JustForRecord;
            recurringTransaction.Type = request.Type;
            recurringTransaction.StartDate = request.StartDate.Date;
            recurringTransaction.EndDate = request.EndDate?.Date;
            recurringTransaction.AccountId = request.AccountId;
            recurringTransaction.CategoryId = request.CategoryId;
            recurringTransaction.IsActive = request.IsActive;
            recurringTransaction.UpdatedAt = DateTime.UtcNow;
            ApplyRule(recurringTransaction.RecurrenceRule, request.RecurrenceRule);

            await _context.SaveChangesAsync();
            return _mapper.Map<InfoRecurringTransactionResponse>(recurringTransaction);
        }

        public async Task<Result<bool>> DeleteRecurringTransactionAsync(long recurringId, int userId)
        {
            var recurringTransaction = await _context.RecurringTransactions.Include(transaction => transaction.RecurrenceRule)
                .FirstOrDefaultAsync(transaction => transaction.Id == recurringId && transaction.UserId == userId);
            if (recurringTransaction is null) return new AppError("Transação recorrente não encontrada.", ErrorTypeEnum.NotFound);

            await using var databaseTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var recurrenceRule = recurringTransaction.RecurrenceRule;
                _context.RecurringTransactions.Remove(recurringTransaction);
                await _context.SaveChangesAsync();
                if (!await _context.RecurringTransactions.AnyAsync(transaction => transaction.RecurrenceRuleId == recurrenceRule.Id))
                {
                    _context.RecurrenceRules.Remove(recurrenceRule);
                    await _context.SaveChangesAsync();
                }
                await databaseTransaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await databaseTransaction.RollbackAsync();
                return new AppError("Não foi possível remover a transação recorrente.", ErrorTypeEnum.InternalError);
            }
        }

        public async Task<List<InfoRecurringTransactionInstanceResponse>> GetRecurringTransactionInstancesAsync(InstanceStatusEnum status, int userId, DateTime? startDate, DateTime? endDate)
        {
            var query = _context.RecurringTransactionInstances.AsNoTracking()
                .Include(instance => instance.RecurringTransaction)
                .Where(instance => instance.Status == status && instance.RecurringTransaction.UserId == userId);
            if (startDate.HasValue) query = query.Where(instance => instance.ScheduledDate >= startDate.Value.Date);
            if (endDate.HasValue) query = query.Where(instance => instance.ScheduledDate <= endDate.Value.Date);
            var instances = await query.OrderByDescending(instance => instance.ScheduledDate).ToListAsync();
            return _mapper.Map<List<InfoRecurringTransactionInstanceResponse>>(instances);
        }

        public async Task<Result<List<InfoRecurringTransactionInstanceResponse>>> ProcessPendingRecurringTransactionInstances(List<long> pendingTransactions, InstanceStatusEnum action, int userId, string? rejectReason)
        {
            if (pendingTransactions is null || pendingTransactions.Count == 0) return new AppError("Informe ao menos uma pendência.", ErrorTypeEnum.Validation);
            if (action is not InstanceStatusEnum.CONFIRMED and not InstanceStatusEnum.REJECTED) return new AppError("A pendência só pode ser confirmada ou recusada.", ErrorTypeEnum.BusinessRule);

            var ids = pendingTransactions.Distinct().ToList();
            var instances = await _context.RecurringTransactionInstances.Include(instance => instance.RecurringTransaction)
                .Where(instance => ids.Contains(instance.Id) && instance.Status == InstanceStatusEnum.PENDING && instance.RecurringTransaction.UserId == userId)
                .ToListAsync();
            if (instances.Count != ids.Count) return new AppError("Uma ou mais pendências não foram encontradas ou já foram processadas.", ErrorTypeEnum.NotFound);

            await using var databaseTransaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var instance in instances)
                {
                    instance.ProcessedDate = DateTime.UtcNow;
                    instance.UpdatedAt = DateTime.UtcNow;
                    if (action == InstanceStatusEnum.REJECTED)
                    {
                        instance.Status = InstanceStatusEnum.REJECTED;
                        instance.RejectionReason = string.IsNullOrWhiteSpace(rejectReason) ? null : rejectReason.Trim();
                        continue;
                    }

                    var request = new CreateTransactionRequest
                    {
                        AccountId = instance.RecurringTransaction.AccountId,
                        Amount = instance.RecurringTransaction.Amount,
                        CategoryId = instance.RecurringTransaction.CategoryId,
                        Description = instance.RecurringTransaction.Description,
                        Destination = instance.RecurringTransaction.Destination ?? string.Empty,
                        JustForRecord = instance.RecurringTransaction.JustForRecord,
                        PurchaseDate = instance.ScheduledDate,
                        Type = instance.RecurringTransaction.Type,
                    };
                    var result = await _transactionService.CreateTransactionAsync(request, userId);
                    if (result.IsError)
                    {
                        instance.Status = InstanceStatusEnum.ERROR;
                        instance.RejectionReason = result.Error.ErrorMessage;
                        continue;
                    }
                    instance.Status = InstanceStatusEnum.CONFIRMED;
                    instance.ActualTransactionId = result.Value.Id;
                    instance.RejectionReason = null;
                }
                await _context.SaveChangesAsync();
                await databaseTransaction.CommitAsync();
                return _mapper.Map<List<InfoRecurringTransactionInstanceResponse>>(instances);
            }
            catch (Exception)
            {
                await databaseTransaction.RollbackAsync();
                return new AppError("Não foi possível processar as pendências.", ErrorTypeEnum.InternalError);
            }
        }

        public Task<List<RecurringTransaction>> GetRecurringTransactionsToProcessAsync(DateTime targetDate)
        {
            var date = targetDate.Date;
            var dayOfWeek = (int)date.DayOfWeek;
            var isLastDayOfMonth = date.Day == DateTime.DaysInMonth(date.Year, date.Month);
            return _context.RecurringTransactions.Include(transaction => transaction.RecurrenceRule)
                .Where(transaction => transaction.IsActive && transaction.StartDate <= date && (transaction.EndDate == null || transaction.EndDate >= date) && !transaction.Instances.Any(instance => instance.ScheduledDate == date))
                .Where(transaction =>
                    (transaction.RecurrenceRule.Frequency == RecurrenceFrequencyEnum.DAILY && (transaction.RecurrenceRule.IsEveryDay || (transaction.RecurrenceRule.DaysOfWeek != null && transaction.RecurrenceRule.DaysOfWeek.Contains(dayOfWeek.ToString()))))
                    || (transaction.RecurrenceRule.Frequency == RecurrenceFrequencyEnum.WEEKLY && transaction.RecurrenceRule.DayOfWeek == dayOfWeek && IsEveryWeeks(transaction.RecurrenceRule, transaction.StartDate, date))
                    || (transaction.RecurrenceRule.Frequency == RecurrenceFrequencyEnum.MONTHLY && (transaction.RecurrenceRule.DayOfMonth == date.Day || (transaction.RecurrenceRule.DayOfMonth == -1 && isLastDayOfMonth)) && IsEveryMonths(transaction.RecurrenceRule, transaction.StartDate, date))
                    || (transaction.RecurrenceRule.Frequency == RecurrenceFrequencyEnum.YEARLY && transaction.RecurrenceRule.MonthOfYear == date.Month && transaction.RecurrenceRule.DayOfMonthForYearly == date.Day && IsEveryYears(transaction.RecurrenceRule, transaction.StartDate, date)))
                .ToListAsync();
        }

        [AutomaticRetry(Attempts = 3)]
        [DisableConcurrentExecution(timeoutInSeconds: 3600)]
        public async Task ProcessRecurringTransactionsAsync()
        {
            var today = DateTime.UtcNow.Date;
            foreach (var transaction in await GetRecurringTransactionsToProcessAsync(today))
            {
                try
                {
                    await _context.RecurringTransactionInstances.AddAsync(new RecurringTransactionInstance { RecurringTransactionId = transaction.Id, ScheduledDate = today, Status = InstanceStatusEnum.PENDING, CreatedAt = DateTime.UtcNow });
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    _context.ChangeTracker.Clear();
                }
            }
        }

        private async Task<Result<bool>> ValidateRequestAsync(CreateRecurringTransactionRequest request, int userId)
        {
            var validation = RecurringTransactionValidations.ValidateCreateRecurringTransactionRequest(request);
            if (validation.IsError) return validation;
            if (!await _context.Accounts.AnyAsync(account => account.Id == request.AccountId && account.UserId == userId && !account.Deleted)) return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
            var category = await _context.Categories.FirstOrDefaultAsync(item => item.Id == request.CategoryId && item.UserId == userId);
            if (category is null) return new AppError("Categoria não encontrada.", ErrorTypeEnum.NotFound);
            var expectedBillType = request.Type == TransactionTypeEnum.EXPENSE ? BillTypeEnum.EXPENSE : BillTypeEnum.INCOME;
            return category.BillType == expectedBillType ? true : new AppError("Categoria incompatível com o tipo da transação.", ErrorTypeEnum.BusinessRule);
        }

        private async Task<Result<bool>> ValidateUpdateRequestAsync(UpdateRecurringTransactionRequest request, int userId)
        {
            var validation = RecurringTransactionValidations.ValidateUpdateRecurringTransactionRequest(request);
            if (validation.IsError) return validation;

            if (!await _context.Accounts.AnyAsync(account => account.Id == request.AccountId && account.UserId == userId && !account.Deleted))
            {
                return new AppError("Conta não encontrada.", ErrorTypeEnum.NotFound);
            }

            var category = await _context.Categories.FirstOrDefaultAsync(item => item.Id == request.CategoryId && item.UserId == userId);
            if (category is null)
            {
                return new AppError("Categoria não encontrada.", ErrorTypeEnum.NotFound);
            }

            var expectedBillType = request.Type == TransactionTypeEnum.EXPENSE ? BillTypeEnum.EXPENSE : BillTypeEnum.INCOME;
            return category.BillType == expectedBillType
                ? true
                : new AppError("Categoria incompatível com o tipo da transação.", ErrorTypeEnum.BusinessRule);
        }

        private static RecurrenceRule CreateRule(CreateRecurrenceRuleRequest source)
        {
            var rule = new RecurrenceRule { CreatedAt = DateTime.UtcNow };
            ApplyRule(rule, source);
            return rule;
        }

        private static void ApplyRule(RecurrenceRule target, CreateRecurrenceRuleRequest source)
        {
            target.Frequency = source.Frequency;
            target.Interval = source.Interval;
            target.IsEveryDay = source.Frequency == RecurrenceFrequencyEnum.DAILY && source.IsEveryDay;
            target.DaysOfWeek = source.Frequency == RecurrenceFrequencyEnum.DAILY && !source.IsEveryDay ? source.DaysOfWeek : null;
            target.DayOfWeek = source.Frequency == RecurrenceFrequencyEnum.WEEKLY ? source.DayOfWeek : null;
            target.DayOfMonth = source.Frequency == RecurrenceFrequencyEnum.MONTHLY ? source.DayOfMonth : null;
            target.MonthOfYear = source.Frequency == RecurrenceFrequencyEnum.YEARLY ? source.MonthOfYear : null;
            target.DayOfMonthForYearly = source.Frequency == RecurrenceFrequencyEnum.YEARLY ? source.DayOfMonthForYearly : null;
        }

        private static bool IsEveryWeeks(RecurrenceRule rule, DateTime start, DateTime target) => ((target.Date - start.Date).Days / 7) % rule.Interval == 0;
        private static bool IsEveryMonths(RecurrenceRule rule, DateTime start, DateTime target)
        {
            var firstOccurrenceMonth = start.Date;
            var occurrenceDay = rule.DayOfMonth == -1
                ? DateTime.DaysInMonth(start.Year, start.Month)
                : rule.DayOfMonth!.Value;
            if (start.Day > occurrenceDay)
            {
                firstOccurrenceMonth = firstOccurrenceMonth.AddMonths(1);
            }

            var elapsedMonths = (target.Year - firstOccurrenceMonth.Year) * 12 + target.Month - firstOccurrenceMonth.Month;
            return elapsedMonths % rule.Interval == 0;
        }

        private static bool IsEveryYears(RecurrenceRule rule, DateTime start, DateTime target)
        {
            var firstOccurrenceYear = start.Year;
            var occurrence = new DateTime(firstOccurrenceYear, rule.MonthOfYear!.Value, rule.DayOfMonthForYearly!.Value);
            if (occurrence < start.Date)
            {
                firstOccurrenceYear++;
            }

            return (target.Year - firstOccurrenceYear) % rule.Interval == 0;
        }
    }
}
