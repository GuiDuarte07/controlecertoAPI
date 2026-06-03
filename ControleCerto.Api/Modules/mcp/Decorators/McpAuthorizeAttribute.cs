using ControleCerto.Modules.Mcp.DTOs;
using ControleCerto.Modules.Mcp.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ControleCerto.Modules.Mcp.Decorators
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class McpAuthorizeAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var authorizationService = context.HttpContext.RequestServices.GetService<IMcpAuthorizationService>();

            if (authorizationService is null)
            {
                context.Result = new UnauthorizedObjectResult(
                    McpCommandResponse.FromError("authorization", "Serviço de autorização MCP não configurado."));
                return;
            }

            var validationResult = await authorizationService.ValidateAndAttachClaimsAsync(
                context.HttpContext,
                context.HttpContext.RequestAborted);

            if (validationResult.IsError)
            {
                context.Result = new UnauthorizedObjectResult(
                    McpCommandResponse.FromError("authorization", validationResult.Error.ErrorMessage));
                return;
            }

            await next();
        }
    }
}
