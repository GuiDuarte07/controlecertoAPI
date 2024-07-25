using Finantech.DTOs.User;
using Finantech.Extensions;
using Finantech.Services.Interfaces;
using Finantech.Utils;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [AllowAnonymous]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest data)
        {
            var result = await _userService.CreateUserAync(data);

            if (result.IsSuccess)
            {
                return Created("Auth/Authenticate", result.Value);
            }

            return result.HandleReturnResult();

        }

        [AllowAnonymous]
        [HttpGet("ConfirmEmail/{token}")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var result = await _userService.ConfirmEmail(token);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpGet("GenerateConfirmEmailToken/{userId}")]
        public async Task<IActionResult> GenerateConfirmEmailToken(int userId)
        {
            var result = await _userService.GenerateConfirmEmailToken(userId);

            return result.HandleReturnResult();
        }
    }
}
