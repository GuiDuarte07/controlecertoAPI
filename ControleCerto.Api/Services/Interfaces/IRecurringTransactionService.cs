using ControleCerto.Models.Entities;

namespace ControleCerto.Services.Interfaces
{
    public interface IRecurringTransactionService
    {
        public Task<List<RecurringTransaction>> GetRecurringTransactionsToProcessAsync(DateTime targetDate);
        public Task ProcessRecurringTransactionsAsync();

    }
}
