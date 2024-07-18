using Finantech.DTOs.Transaction;
using Finantech.DTOs.TransferenceDTO;
using Finantech.Models.DTOs;

namespace Finantech.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<InfoTransactionResponse> CreateTransactionAsync(CreateTransactionRequest request, int userId);
        public Task DeleteTransactionAsync(int expenseId, int userId);
        public Task<InfoTransactionResponse> UpdateTransactionAsync(UpdateTransactionRequest request, int userId);
        public Task<InfoTransferenceResponse> CreateTransferenceAsync(CreateTransferenceRequest request, int userId);
        public Task<TransactionList> GetTransactionsAsync(int userId, DateTime startDate, DateTime endDate, int? accountId);
    }
}
