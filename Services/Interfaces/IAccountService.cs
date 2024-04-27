using Finantech.DTOs.Account;
using Finantech.Models.DTOs;
using Finantech.Models.Entities;

namespace Finantech.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<InfoAccountResponse> CreateAccountAsync(CreateAccountRequest accountRequest, int userId);
        public Task<InfoAccountResponse> UpdateAccountAsync(UpdateAccountRequest request, int userId);
        public Task<ICollection<InfoAccountResponse>> GetAccountsByUserIdAsync(int userId);
    }
}
