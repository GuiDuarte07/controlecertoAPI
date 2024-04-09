using Finantech.DTOs.User;
using Finantech.Models.AppDbContext;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace Finantech.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        private readonly IHashService _hashService;

        public AuthService(AppDbContext appDbContext, IConfiguration configuration, IHashService hashService)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _hashService = hashService;
        }

        public InfoUserResponse? Authenticate(string email, string password)
        {
            // FAZER O HASH
            string passwordHash = _hashService.HashPassword(password);

            var user = _appDbContext.Users.FirstOrDefault(user => user.Email == email);

            if (user == null || user.PasswordHash != passwordHash)
            {
                return null;
            }

            // FAZER O MAP
            var userResponse = new InfoUserResponse();

            return userResponse;
        }

        public string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim("email", user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var privateKey = new SymmetricSecurityKey(Encoding.UTF8.
            GetBytes(_configuration["jwt:secretKey"]!));

            var credentials = new SigningCredentials(privateKey, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddYears(1);

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _configuration["jwt:issuer"],
                audience: _configuration["jwt:audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
