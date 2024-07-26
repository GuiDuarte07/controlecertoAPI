using Microsoft.Extensions.Caching.Distributed;

namespace Finantech.Services.Interfaces
{
    public interface ICacheService
    {
        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options);
        public Task<T?> GetAsync<T>(string key);
        public Task RemoveAsync(string key);
        public Task SetRefreshTokenAsync(string userId, string refreshToken);
        public Task<string?> GetRefreshTokenAsync(string refreshToken);
        public Task RemoveRefreshTokenAsync(string refreshToken);

        public Task SetConfirmEmailTokenAsync(string email, string confirmEmailToken);
        public Task<string?> GetConfirmEmailTokenAsync(string confirmEmailToken);
        public Task RemoveConfirmEmailTokenAsync(string confirmEmailToken);

        public Task SetForgotPasswordTokenAsync(string email, string forgotPasswordToken);
        public Task<string?> GetForgotPasswordTokenAsync(string forgotPasswordToken);
        public Task RemoveForgotPasswordTokenAsync(string forgotPasswordToken);
    }
}
