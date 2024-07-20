using Finantech.DTOs.Account;
using Finantech.Errors;
using Finantech.Models.DTOs;
using Finantech.Models.Entities;
using Npgsql.PostgresTypes;

namespace Finantech.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<Result<InfoAccountResponse>> CreateAccountAsync(CreateAccountRequest accountRequest, int userId);
        public Task<Result<InfoAccountResponse>> UpdateAccountAsync(UpdateAccountRequest request, int userId);
        public Task<Result<bool>> DeleteAccountAsync(int accountId, int userId);
        public Task<Result<ICollection<InfoAccountResponse>>> GetAccountsByUserIdAsync(int userId);
        public Task<Result<ICollection<InfoAccountResponse>>> GetAccountsWithoutCreditCardAsync(int userId);
        public Task<Result<BalanceStatement>> GetBalanceStatementAsync(int userId, DateTime? startDate, DateTime? endDate);
        public Task<Result<double>> GetAccountBalanceAsync(int userId);
    }
}
