using Finantech.DTOs.Account;
using Finantech.Models.DTOs;
using Finantech.Models.Entities;
using Npgsql.PostgresTypes;

namespace Finantech.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<InfoAccountResponse> CreateAccountAsync(CreateAccountRequest accountRequest, int userId);
        public Task<InfoAccountResponse> UpdateAccountAsync(UpdateAccountRequest request, int userId);
        public Task DeleteAccountAsync(int accountId, int userId);
        public Task<ICollection<InfoAccountResponse>> GetAccountsByUserIdAsync(int userId);
        public Task<BalanceStatement> GetBalanceStatementAsync(int userId, DateTime? startDate, DateTime? endDate);
        public Task<double> GetAccountBalanceAsync(int userId);
    }
}
