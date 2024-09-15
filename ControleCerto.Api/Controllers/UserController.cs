using ControleCerto.Decorators;
using ControleCerto.DTOs.Events;
using ControleCerto.DTOs.User;
using ControleCerto.Extensions;
using ControleCerto.Services.Interfaces;
using ControleCerto.Utils;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace ControleCerto.Controllers
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
        
        [Authorize]
        [ExtractTokenInfo]
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;
            
            var result = await _userService.GetUserDetails(userId);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest data)
        {
            var result = await _userService.CreateUserAync(data);

            return result.IsSuccess ? Created("Auth/Authenticate", result.Value) : result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpGet("ConfirmEmail/{token}")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var result = await _userService.ConfirmEmailAsync(token);

            return result.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpGet("SendConfirmEmail")]
        public async Task<IActionResult> GenerateConfirmEmailToken()
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _userService.GenerateConfirmEmailTokenAsync(userId);

            return result.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _userService.ChangePasswordAsync(changePasswordRequest, userId);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpPost("SendForgotPasswordEmail")]
        public async Task<IActionResult> SendForgotPasswordEmail([FromBody] ForgotPasswordEvent forgotPasswordEmail)
        {
            var result = await _userService.GenerateForgotPasswordTokenAsync(forgotPasswordEmail.Email);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpGet("VerifyForgotPasswordToken/{token}")]
        public async Task<IActionResult> VerifyForgotPasswordToken([FromRoute] string token)
        {
            var result = await _userService.VerifyForgotPasswordTokenAsync(token);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword/{token}")]
        public async Task<IActionResult> ForgotPassword([FromRoute] string token, [FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            var result = await _userService.ForgotPasswordAsync(token, forgotPasswordRequest);

            return result.HandleReturnResult();
        }
    }
}
