using Finantech.DTOs.Transaction;
using Finantech.DTOs.TransferenceDTO;
using Finantech.Errors;
using Finantech.Models.DTOs;

namespace Finantech.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<Result<InfoTransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request, int userId);
        public Task<Result<bool>> DeleteTransactionAsync(int expenseId, int userId);
        public Task<Result<InfoTransactionResponse>> UpdateTransactionAsync(UpdateTransactionRequest request, int userId);
        public Task<Result<InfoTransferenceResponse>> CreateTransferenceAsync(CreateTransferenceRequest request, int userId);
        public Task<Result<TransactionList>> GetTransactionsAsync(int userId, DateTime startDate, DateTime endDate, int? accountId);
    }
}
