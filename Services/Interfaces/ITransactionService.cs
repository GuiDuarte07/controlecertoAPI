using Finantech.DTOs.Account;
using Finantech.DTOs.Expense;
using Finantech.DTOs.Income;
using Finantech.Models.DTOs;

namespace Finantech.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<InfoIncomeResponse> CreateIncomeAsync(CreateIncomeRequest request, int userId);
        public Task<InfoIncomeResponse> UpdateIncomeAsync(UpdateIncomeRequest request, int userId);
        public Task DeleteExpenseAsync(int expenseId, int userId);
        public Task<InfoExpenseResponse> CreateExpenseAsync(CreateExpenseRequest request, int userId);
        public Task<InfoExpenseResponse> UpdateExpenseAsync(UpdateExpenseRequest request, int userId);
        public Task DeleteIncomeAsync(int incomeId, int userId);
        public Task<IEnumerable<InfoTransactionResponse>> GetTransactionsWithPaginationAsync(int pageNumber, int pageSize, int userId, DateTime startDate, DateTime endDate, int? accountId);
    }
}
