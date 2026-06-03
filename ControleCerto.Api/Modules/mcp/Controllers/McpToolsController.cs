using ControleCerto.Modules.Mcp.Decorators;
using ControleCerto.Modules.Mcp.DTOs;
using ControleCerto.Modules.Mcp.Models;
using ControleCerto.Modules.Mcp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Modules.Mcp.Controllers
{
    [ApiController]
    [Route("api/mcp/tools")]
    [McpAuthorize]
    public class McpToolsController : ControllerBase
    {
        private readonly IToolRegistryService _toolRegistryService;

        public McpToolsController(IToolRegistryService toolRegistryService)
        {
            _toolRegistryService = toolRegistryService;
        }

        [HttpGet]
        public IActionResult GetTools()
        {
            var tokenClaims = HttpContext.Items[McpHttpContextKeys.Claims] as McpTokenClaims;
            if (tokenClaims is null)
            {
                return Unauthorized(McpCommandResponse.FromError("authorization", "Token MCP inválido."));
            }

            var tools = _toolRegistryService
                .GetToolsForPermissions(tokenClaims.Permissions)
                .Select(tool => new ToolMetadataResponse
                {
                    Name = tool.Name,
                    Resource = tool.Resource,
                    Action = tool.Action,
                    Description = tool.Description,
                    Parameters = tool.Parameters,
                    RequiredPermissions = tool.RequiredPermissions
                })
                .ToArray();

            return Ok(tools);
        }
    }
}
