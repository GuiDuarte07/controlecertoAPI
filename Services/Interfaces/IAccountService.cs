using Finantech.DTOs.Account;
using Finantech.Models.DTOs;

namespace Finantech.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<InfoAccountResponse> CreateAccountAsync(CreateAccountRequest accountRequest);
        public Task<ICollection<InfoTransactionResponse>> GetTransactionsWithPagination(int userId, int? accountId, int pageNumber, int pageSize);
    }
}
