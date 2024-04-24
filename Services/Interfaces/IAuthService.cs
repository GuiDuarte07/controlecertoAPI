using Finantech.DTOs.Auth;
using Finantech.DTOs.User;
using Finantech.Models.Entities;

namespace Finantech.Services.Interfaces
{
    public interface IAuthService
    {
        public AuthResponse? Authenticate(string email, string password);
        public string GenerateToken(User user);
    }
}
