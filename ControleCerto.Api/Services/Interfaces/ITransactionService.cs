using ControleCerto.DTOs.Transaction;
using ControleCerto.DTOs.TransferenceDTO;
using ControleCerto.Errors;
using ControleCerto.Models.DTOs;

namespace ControleCerto.Services.Interfaces
{
    public interface ITransactionService
    {
        public Task<Result<InfoTransactionResponse>> CreateTransactionAsync(CreateTransactionRequest request, int userId);
        public Task<Result<bool>> DeleteTransactionAsync(int expenseId, int userId);
        public Task<Result<InfoTransactionResponse>> UpdateTransactionAsync(UpdateTransactionRequest request, int userId);
        public Task<Result<InfoTransferenceResponse>> CreateTransferenceAsync(CreateTransferenceRequest request, int userId);
        public Task<Result<TransactionList>> GetTransactionsAsync(int userId, DateTime startDate, DateTime endDate, bool? seeInvoices,int? accountId);
    }
}
