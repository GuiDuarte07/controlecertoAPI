using ControleCerto.Decorators;
using ControleCerto.DTOs.Auth;
using ControleCerto.Extensions;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [AllowAnonymous]
        [HttpPost("token")]
        public async Task<IActionResult> AuthenticateAsync([FromBody] AuthRequest data)
        {
            var authInfo = await _authService.AuthenticateAsync(data.Email, data.Password);

            return authInfo.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpPost("token/refresh/{refreshToken}")]
        public async Task<IActionResult> GenerateAccessToken(string refreshToken)
        {
            var authResult = await _authService.GenerateAccessTokenAsync(refreshToken);

            return authResult.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpPost("logout/{refreshToken}")]
        public async Task<IActionResult> Logout(string refreshToken)
        {
            await _authService.Logout(refreshToken);

            return Ok();
        }
    }
}
