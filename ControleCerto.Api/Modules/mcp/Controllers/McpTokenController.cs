using ControleCerto.Decorators;
using ControleCerto.Extensions;
using ControleCerto.Modules.Mcp.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.Mcp.Controllers
{
    [ApiController]
    [Route("api/mcp")]
    [Authorize]
    [ExtractTokenInfo]
    public class McpTokenController : ControllerBase
    {
        private readonly IMcpAuthorizationService _mcpAuthorizationService;

        public McpTokenController(IMcpAuthorizationService mcpAuthorizationService)
        {
            _mcpAuthorizationService = mcpAuthorizationService;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GenerateToken(CancellationToken cancellationToken)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _mcpAuthorizationService.GenerateTokenAsync(userId, cancellationToken);
            return result.HandleReturnResult();
        }
    }
}
