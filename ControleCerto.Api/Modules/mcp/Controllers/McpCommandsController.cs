using ControleCerto.Modules.Mcp.Decorators;
using ControleCerto.Modules.Mcp.DTOs;
using ControleCerto.Modules.Mcp.Models;
using ControleCerto.Modules.Mcp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.Mcp.Controllers
{
    [ApiController]
    [Route("api/mcp/commands")]
    [McpAuthorize]
    public class McpCommandsController : ControllerBase
    {
        private readonly IMcpCommandService _mcpCommandService;

        public McpCommandsController(IMcpCommandService mcpCommandService)
        {
            _mcpCommandService = mcpCommandService;
        }

        [HttpPost]
        public async Task<IActionResult> ExecuteCommand([FromBody] McpCommandRequest request, CancellationToken cancellationToken)
        {
            var tokenClaims = HttpContext.Items[McpHttpContextKeys.Claims] as McpTokenClaims;
            if (tokenClaims is null)
            {
                return Unauthorized(McpCommandResponse.FromError("authorization", "Token MCP inválido."));
            }

            var response = await _mcpCommandService.ExecuteAsync(request, tokenClaims, cancellationToken);
            return Ok(response);
        }
    }
}
