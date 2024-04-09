using System.Text;
using System.Security.Cryptography;
using Finantech.Services.Interfaces;

namespace Finantech.Services
{
    public class HashService : IHashService
    {
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}