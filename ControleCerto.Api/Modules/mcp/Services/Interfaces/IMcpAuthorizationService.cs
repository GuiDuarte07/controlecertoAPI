using ControleCerto.Errors;
using ControleCerto.Modules.Mcp.DTOs;
using ControleCerto.Modules.Mcp.Models;

namespace ControleCerto.Modules.Mcp.Services.Interfaces
{
    public interface IMcpAuthorizationService
    {
        Task<Result<McpTokenClaims>> ValidateAndAttachClaimsAsync(HttpContext httpContext, CancellationToken cancellationToken = default);
        Task<Result<McpTokenResponse>> GenerateTokenAsync(int userId, CancellationToken cancellationToken = default);
        bool HasRequiredPermissions(McpTokenClaims tokenClaims, IEnumerable<string> requiredPermissions);
    }
}
