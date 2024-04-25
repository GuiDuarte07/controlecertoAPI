using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Finantech.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Finantech.Decorators
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class ExtractTokenInfoAttribute : Attribute, IAsyncActionFilter
    {
        public TokenInfoDTO TokenInfo { get; private set; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;
            var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
                var claims = jwtToken!.Claims;

                httpContext.Items["UserId"] = int.Parse(claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "-1");
                httpContext.Items["Email"] = claims.FirstOrDefault(c => c.Type == "email")?.Value!;

                // Extrair as informações relevantes do token e armazenar em uma variável normal
                TokenInfo = new TokenInfoDTO
                {
                    UserId = int.Parse(claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0"),
                    Email = claims.FirstOrDefault(c => c.Type == "email")?.Value!
                };

                // Ou, se desejar, pode armazenar as informações individualmente
                //UserId = int.Parse(claims.FirstOrDefault(c => c.Type == "id")?.Value ?? "0");
                //Email = claims.FirstOrDefault(c => c.Type == "email")?.Value;
                //AccountId = int.Parse(claims.FirstOrDefault(c => c.Type == "accountId")?.Value ?? "0");
            }
            catch (Exception)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            await next();
        }
    }

}
