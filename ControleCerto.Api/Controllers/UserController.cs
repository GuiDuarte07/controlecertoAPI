using ControleCerto.Decorators;
using ControleCerto.DTOs.Events;
using ControleCerto.DTOs.User;
using ControleCerto.Errors;
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
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }
        
        [Authorize]
        [ExtractTokenInfo]
        [HttpGet("me")]
        public async Task<IActionResult> GetUser()
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;
            
            var result = await _userService.GetUserDetails(userId);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest data)
        {
            var result = await _userService.CreateUserAync(data);

            return result.IsSuccess ? Created($"/api/users/{result.Value.Id}", result.Value) : result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpGet("confirm-email/{token}")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var result = await _userService.ConfirmEmailAsync(token);

            return result.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpPost("me/confirm-email")]
        public async Task<IActionResult> GenerateConfirmEmailToken()
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _userService.GenerateConfirmEmailTokenAsync(userId);

            return result.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpPost("me/password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _userService.ChangePasswordAsync(changePasswordRequest, userId);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpPost("password/forgot")]
        public async Task<IActionResult> SendForgotPasswordEmail([FromBody] ForgotPasswordEvent forgotPasswordEmail)
        {
            var result = await _userService.GenerateForgotPasswordTokenAsync(forgotPasswordEmail.Email);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpGet("password/forgot/{token}")]
        public async Task<IActionResult> VerifyForgotPasswordToken([FromRoute] string token)
        {
            var result = await _userService.VerifyForgotPasswordTokenAsync(token);

            return result.HandleReturnResult();
        }

        [AllowAnonymous]
        [HttpPost("password/forgot/{token}")]
        public async Task<IActionResult> ForgotPassword([FromRoute] string token, [FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            var result = await _userService.ForgotPasswordAsync(token, forgotPasswordRequest);

            return result.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpPatch("me")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
        int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            if (!ModelState.IsValid)
            {
                var errorResponse = ErrorResponse.FromModelState(ModelState);
                return StatusCode(errorResponse.Code, errorResponse);
            }

            var result = await _userService.UpdateUserAsync(request, userId);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }
            else
            {
                return result.HandleReturnResult();
            }
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpDelete("me")]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _userService.DeleteUserAsync(userId);

            return result.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpPost("me/reset")]
        public async Task<IActionResult> ResetUserData([FromBody] ResetUserDataRequest request)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _userService.ResetUserDataAsync(request, userId);

            return result.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpPost("me/avatar")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadAvatar([FromForm] IFormFile file)
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _userService.UploadAvatarAsync(file, userId);

            return result.HandleReturnResult();
        }

        [Authorize]
        [ExtractTokenInfo]
        [HttpDelete("me/avatar")]
        public async Task<IActionResult> DeleteAvatar()
        {
            var userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _userService.DeleteAvatarAsync(userId);

            return result.HandleReturnResult();
        }
    }
}
