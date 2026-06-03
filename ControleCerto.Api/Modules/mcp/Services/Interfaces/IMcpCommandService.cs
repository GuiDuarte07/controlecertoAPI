using ControleCerto.Modules.Mcp.DTOs;
using ControleCerto.Modules.Mcp.Models;

namespace ControleCerto.Modules.Mcp.Services.Interfaces
{
    public interface IMcpCommandService
    {
        Task<McpCommandResponse> ExecuteAsync(McpCommandRequest request, McpTokenClaims tokenClaims, CancellationToken cancellationToken = default);
    }
}
