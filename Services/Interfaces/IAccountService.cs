using Finantech.DTOs.Account;

namespace Finantech.Services.Interfaces
{
    public interface IAccountService
    {
        public Task<InfoAccountResponse> CreateAccountAsync(CreateAccountRequest accountRequest);
    }
}
