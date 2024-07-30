using Finantech.DTOs.Auth;
using Finantech.Errors;

namespace Finantech.Services.Interfaces
{
    public interface IAuthService
    {
        public Task<Result<AuthResponse>> AuthenticateAsync(string email, string password);
        public Task<Result<AccessTokenRequest>> GenerateAccessTokenAsync(string refreshToken);
        public Task Logout(string refreshToken);
    }
}
