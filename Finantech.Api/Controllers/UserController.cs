using Finantech.DTOs.User;
using Finantech.Extensions;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
