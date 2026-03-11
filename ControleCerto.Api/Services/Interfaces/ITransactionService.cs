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
        
        /// <summary>
        /// Get transactions in two modes:
        /// - "invoice": Monthly invoice view with transactions grouped by invoice
        /// - "statement": Detailed ledger view with all transactions, advanced filters and sorting
        /// </summary>
        public Task<Result<object>> GetTransactionsAsync(
            int userId, 
            DateTime startDate, 
            DateTime endDate, 
            string mode = "statement",
            int? accountId = null,
            long? cardId = null,
            long? categoryId = null,
            string? searchText = null,
            string? sort = null,
            int pageNumber = 1,
            int pageSize = 20
        );
    }
}
