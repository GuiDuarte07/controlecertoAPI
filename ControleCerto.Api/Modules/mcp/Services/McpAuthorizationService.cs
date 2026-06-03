using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Models.AppDbContext;
using ControleCerto.Modules.Mcp.DTOs;
using ControleCerto.Modules.Mcp.Models;
using ControleCerto.Modules.Mcp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ControleCerto.Modules.Mcp.Services
{
    public class McpAuthorizationService : IMcpAuthorizationService
    {
        private const string McpTokenType = "mcp";
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;
        private readonly IToolRegistryService _toolRegistryService;
        private readonly ILogger<McpAuthorizationService> _logger;

        public McpAuthorizationService(
            AppDbContext appDbContext,
            IConfiguration configuration,
            IToolRegistryService toolRegistryService,
            ILogger<McpAuthorizationService> logger)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
            _toolRegistryService = toolRegistryService;
            _logger = logger;
        }

        public async Task<Result<McpTokenClaims>> ValidateAndAttachClaimsAsync(
            HttpContext httpContext,
            CancellationToken cancellationToken = default)
        {
            var token = ExtractBearerToken(httpContext.Request.Headers.Authorization);
            if (token is null)
            {
                return new AppError("Token MCP não informado.", ErrorTypeEnum.Validation);
            }

            var secretKey = ResolveSecretKey();
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogError("Chave JWT não configurada para validação de token MCP.");
                return new AppError("Servidor sem configuração de segurança para MCP.", ErrorTypeEnum.InternalError);
            }

            ClaimsPrincipal principal;
            JwtSecurityToken jwtToken;

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                jwtToken = (JwtSecurityToken)validatedToken;
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                _logger.LogWarning(ex, "Falha ao validar token MCP.");
                return new AppError("Token MCP inválido ou expirado.", ErrorTypeEnum.Validation);
            }

            var tokenType = principal.Claims.FirstOrDefault(c => c.Type == "type")?.Value;
            if (!string.Equals(tokenType, McpTokenType, StringComparison.OrdinalIgnoreCase))
            {
                return new AppError("Token inválido para o servidor MCP.", ErrorTypeEnum.Validation);
            }

            var userId = ExtractUserId(principal);
            if (userId <= 0)
            {
                return new AppError("Token MCP sem usuário válido.", ErrorTypeEnum.Validation);
            }

            var userExists = await _appDbContext.Users
                .AsNoTracking()
                .AnyAsync(user => user.Id == userId && !user.Deleted, cancellationToken);

            if (!userExists)
            {
                return new AppError("Usuário do token MCP não existe ou está inativo.", ErrorTypeEnum.NotFound);
            }

            var permissions = ExtractPermissions(principal);
            var claims = new McpTokenClaims
            {
                UserId = userId,
                TokenType = tokenType ?? McpTokenType,
                IssuedAtUtc = ExtractIssuedAtUtc(principal, jwtToken),
                ExpiresAtUtc = jwtToken.ValidTo,
                Permissions = permissions
            };

            httpContext.Items["UserId"] = userId;
            httpContext.Items[McpHttpContextKeys.Claims] = claims;

            return claims;
        }

        public async Task<Result<McpTokenResponse>> GenerateTokenAsync(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(currentUser => currentUser.Id == userId && !currentUser.Deleted, cancellationToken);

            if (user is null)
            {
                return new AppError("Usuário não encontrado para emissão do token MCP.", ErrorTypeEnum.NotFound);
            }

            var secretKey = ResolveSecretKey();
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                _logger.LogError("Chave JWT não configurada para emissão de token MCP.");
                return new AppError("Servidor sem configuração de segurança para MCP.", ErrorTypeEnum.InternalError);
            }

            var permissions = _toolRegistryService
                .GetAllTools()
                .SelectMany(tool => tool.RequiredPermissions)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var now = DateTime.UtcNow;
            var expiresAt = now.AddDays(30);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new("userId", user.Id.ToString()),
                new("type", McpTokenType),
                new(JwtRegisteredClaimNames.Iat, EpochTime.GetIntDate(now).ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(permissions.Select(permission => new Claim("permissions", permission)));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var issuer = _configuration["Jwt:Issuer"] ?? _configuration["jwt:issuer"];
            var audience = _configuration["Jwt:Audience"] ?? _configuration["jwt:audience"] ?? issuer;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: credentials);

            var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Token MCP emitido para UserId={UserId}, expira em {ExpiresAtUtc}.", userId, expiresAt);

            return new McpTokenResponse
            {
                AccessToken = $"Bearer {tokenValue}",
                ExpiresAt = expiresAt,
                TokenType = "Bearer",
                Permissions = permissions
            };
        }

        public bool HasRequiredPermissions(McpTokenClaims tokenClaims, IEnumerable<string> requiredPermissions)
        {
            var permissionSet = new HashSet<string>(tokenClaims.Permissions, StringComparer.OrdinalIgnoreCase);
            return requiredPermissions.All(permissionSet.Contains);
        }

        private static string? ExtractBearerToken(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader))
            {
                return null;
            }

            const string bearerPrefix = "Bearer ";
            if (!authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return authorizationHeader[bearerPrefix.Length..].Trim();
        }

        private string? ResolveSecretKey()
        {
            return _configuration["Jwt:secretKey"] ?? _configuration["jwt:secretKey"];
        }

        private static int ExtractUserId(ClaimsPrincipal principal)
        {
            var userIdClaim = principal.Claims.FirstOrDefault(claim =>
                claim.Type == "userId"
                || claim.Type == "id"
                || claim.Type == JwtRegisteredClaimNames.Sub);

            return int.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
        }

        private static IReadOnlyCollection<string> ExtractPermissions(ClaimsPrincipal principal)
        {
            var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var permissionClaims = principal.Claims
                .Where(claim => claim.Type is "permissions" or "permission")
                .Select(claim => claim.Value);

            foreach (var permissionClaimValue in permissionClaims)
            {
                if (string.IsNullOrWhiteSpace(permissionClaimValue))
                {
                    continue;
                }

                if (permissionClaimValue.TrimStart().StartsWith("[", StringComparison.Ordinal))
                {
                    TryParsePermissionsJson(permissionClaimValue, permissions);
                    continue;
                }

                foreach (var permission in permissionClaimValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    permissions.Add(permission);
                }
            }

            return permissions.ToArray();
        }

        private static void TryParsePermissionsJson(string permissionClaimValue, HashSet<string> permissions)
        {
            try
            {
                var element = JsonSerializer.Deserialize<JsonElement>(permissionClaimValue);
                if (element.ValueKind != JsonValueKind.Array)
                {
                    return;
                }

                foreach (var jsonPermission in element.EnumerateArray())
                {
                    if (jsonPermission.ValueKind == JsonValueKind.String)
                    {
                        var permissionValue = jsonPermission.GetString();
                        if (!string.IsNullOrWhiteSpace(permissionValue))
                        {
                            permissions.Add(permissionValue);
                        }
                    }
                }
            }
            catch (JsonException)
            {
                // Ignore malformed permission claims.
            }
        }

        private static DateTime ExtractIssuedAtUtc(ClaimsPrincipal principal, JwtSecurityToken jwtToken)
        {
            var iatClaim = principal.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Iat)?.Value;
            if (long.TryParse(iatClaim, out var issuedAtUnix))
            {
                return DateTimeOffset.FromUnixTimeSeconds(issuedAtUnix).UtcDateTime;
            }

            return jwtToken.ValidFrom;
        }
    }
}
