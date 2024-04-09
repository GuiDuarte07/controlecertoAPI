using Finantech.DTOs.User;
using Finantech.Models.AppDbContext;
using System.Runtime.CompilerServices;

namespace Finantech.Services
{
    public class AuthService
    {
        private AppDbContext _appDbContext;

        public AuthService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public InfoUserResponse? AuthenticateAsync(string email, string password)
        {
            // FAZER O HASH
            var passwordHash = password;

            var user = _appDbContext.Users.FirstOrDefault(user => user.Email == user.Email);

            if (user == null || user.PasswordHash != passwordHash)
            {
                return null;
            }

            // FAZER O MAP
            var userResponse = new InfoUserResponse();

            return userResponse;
        }

        public string GenerateToken()
        {
            string token = string.Empty;

            return token;
        }
    }
}
