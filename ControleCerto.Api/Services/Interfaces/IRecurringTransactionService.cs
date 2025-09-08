using ControleCerto.DTOs.RecurringTransaction;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.DTOs;
using ControleCerto.Models.Entities;

namespace ControleCerto.Services.Interfaces
{
    public interface IRecurringTransactionService
    {
        public Task<Result<InfoRecurringTransactionResponse>> CreateReccuringTransactionAsync(CreateRecurringTransactionRequest request, int userId);
        public Task<List<InfoRecurringTransactionInstanceResponse>> GetRecurringTransactionInstancesAsync(InstanceStatusEnum status, int userId, DateTime? startDate, DateTime? endDate);
        public Task<Result<List<InfoRecurringTransactionInstanceResponse>>> ProcessPendingRecurringTransactionInstances(List<long> pendingTransactions, InstanceStatusEnum action, int userId, string? rejectReason);
        public Task<List<RecurringTransaction>> GetRecurringTransactionsToProcessAsync(DateTime targetDate);
        public Task ProcessRecurringTransactionsAsync();

    }
}
