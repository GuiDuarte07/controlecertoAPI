using Finantech.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Finantech.Services
{
    public class CacheService : ICacheService
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;

        }

        public async Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            var jsonData = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, jsonData, options);
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            var jsonData = await _cache.GetStringAsync(key);
            return jsonData == null ? default : JsonSerializer.Deserialize<T>(jsonData);
        }

        public async Task RemoveAsync(string key)
        {
            await _cache.RemoveAsync(key);
        }

        private string GenerateRefreshTokenKey(string refreshToken) => $"RefreshToken:{refreshToken}";

        public async Task SetRefreshTokenAsync(string userId, string refreshToken)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7),
            };

            var refreshTokenKey = GenerateRefreshTokenKey(refreshToken);
            await _cache.SetStringAsync(refreshTokenKey, userId, options);
        }

        public async Task<string?> GetRefreshTokenAsync(string refreshToken)
        {
            var refreshTokenKey = GenerateRefreshTokenKey(refreshToken);
            return await _cache.GetStringAsync(refreshTokenKey);
        }

        public async Task RemoveRefreshTokenAsync(string refreshToken)
        {
            var refreshTokenKey = GenerateRefreshTokenKey(refreshToken);
            var userId = await _cache.GetStringAsync(refreshTokenKey);

            if (!string.IsNullOrEmpty(userId))
            {
                await _cache.RemoveAsync(refreshTokenKey);
            }            
        }

        private string GenerateConfirmEmailKey(string confirmEmailToken) => $"ConfirmEmail:{confirmEmailToken}";

        public async Task SetConfirmEmailTokenAsync(string email, string confirmEmailToken)
        {
            var confirmEmailTokenKey = GenerateConfirmEmailKey(confirmEmailToken);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            };

            await _cache.SetStringAsync(confirmEmailTokenKey, email, options);
        }
        public async Task<string?> GetConfirmEmailTokenAsync(string confirmEmailToken)
        {
            var confirmEmailTokenKey = GenerateConfirmEmailKey(confirmEmailToken);
            return await _cache.GetStringAsync(confirmEmailTokenKey);
        }
        public async Task RemoveConfirmEmailTokenAsync(string confirmEmailToken)
        {
            var confirmEmailTokenKey = GenerateConfirmEmailKey(confirmEmailToken);
            var email = await _cache.GetStringAsync(confirmEmailTokenKey);

            if (!string.IsNullOrEmpty(email))
            {
                await _cache.RemoveAsync(confirmEmailTokenKey);
            }
        }


        private string GenerateForgotPasswordKey(string forgotPasswordToken) => $"ForgotPassword:{forgotPasswordToken}";
        public async Task SetForgotPasswordTokenAsync(string email, string forgotPasswordToken)
        {
            var forgotPasswordTokenKey = GenerateForgotPasswordKey(forgotPasswordToken);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            };

            await _cache.SetStringAsync(forgotPasswordTokenKey, email, options);
        }

        public async Task<string?> GetForgotPasswordTokenAsync(string forgotPasswordToken)
        {
            var forgotPasswordTokenKey = GenerateForgotPasswordKey(forgotPasswordToken);
            return await _cache.GetStringAsync(forgotPasswordTokenKey);
        }

        public async Task RemoveForgotPasswordTokenAsync(string forgotPasswordToken)
        {
            var forgotPasswordTokenKey = GenerateForgotPasswordKey(forgotPasswordToken);
            var email = await _cache.GetStringAsync(forgotPasswordTokenKey);

            if (!string.IsNullOrEmpty(email))
            {
                await _cache.RemoveAsync(forgotPasswordTokenKey);
            }
        }
    }

}
