using ControleCerto.DTOs.Auth;
using ControleCerto.DTOs.User;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using ControleCerto.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;

namespace ControleCerto.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        private readonly IHashService _hashService;
        private readonly ICacheService _cacheService;

        public AuthService(AppDbContext appDbContext, IConfiguration configuration, IHashService hashService, ICacheService cacheService)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _hashService = hashService;
            _cacheService = cacheService;
        }

        public async Task<Result<AuthResponse>> AuthenticateAsync(string email, string password)
        {
            string passwordHash = _hashService.HashPassword(password);

            var user = _appDbContext.Users.FirstOrDefault(user => user.Email == email);

            if (user == null || user.PasswordHash != passwordHash)
            {
                return new AppError("E-mail ou senha incorreto.", ErrorTypeEnum.Validation);
            }

            // FAZER O MAP
            var userResponse = new InfoUserResponse { 
                Email = user.Email, 
                EmailConfirmed = user.EmailConfirmed, 
                Id = user.Id, 
                Name = user.Name
            };

            var accessToken = GenerateToken(user);
            var refreshToken = RandomGenerate.Generate32BytesToken();

            try
            {
                await _cacheService.SetRefreshTokenAsync(user.Id.ToString(), refreshToken);
            } catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new AppError("Falha ao registrar o token de acesso, tente fazer o login novamente.", ErrorTypeEnum.InternalError);
            }

            return new AuthResponse { User = userResponse, AccessToken = accessToken, RefreshToken = refreshToken };
        }

        public async Task<Result<AccessTokenRequest>> GenerateAccessTokenAsync(string refreshToken)
        {
            string? userIdString = await _cacheService.GetRefreshTokenAsync(refreshToken);

            if (userIdString is null)
            {
                return new AppError("Refresh Token não identificado, por favor, faça o login novamente.", ErrorTypeEnum.Validation);
            }

            int userId = int.Parse(userIdString);

            var user = await _appDbContext.Users.FirstAsync(u => u.Id == userId);

            if (user is null)
            {
                return new AppError("Usuário não encontrado.", ErrorTypeEnum.NotFound);
            }

            var accessToken = GenerateToken(user);

            return new AccessTokenRequest { AccessToken = accessToken };
        }

        public async Task Logout(string refreshToken)
        {
            await _cacheService.RemoveRefreshTokenAsync(refreshToken);
        }

        private string GenerateToken(User user)
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

            var expiration = DateTime.UtcNow.AddMinutes(15);

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
