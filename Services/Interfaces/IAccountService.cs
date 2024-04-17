using Finantech.DTOs.Account;
using Finantech.Models.DTOs;

namespace Finantech.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<InfoAccountResponse> CreateAccountAsync(CreateAccountRequest accountRequest);
        public Task<ICollection<InfoTransactionResponse>> GetMonthTransactionsAsync(int userId, int? accountId);
        public Task<ICollection<InfoTransactionResponse>> GetTransactionsWithPaginationAsync(int pageNumber, int pageSize, int userId, int? accountId);
        public Task<InfoAccountResponse> UpdateAccountAsync(UpdateAccountRequest request);
        public Task<ICollection<InfoAccountResponse>> GetAccountsByUserIdAsync(int userId);
    }
}
