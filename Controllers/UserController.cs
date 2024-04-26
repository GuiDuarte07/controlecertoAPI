using Finantech.DTOs.User;
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
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest data)
        {
            try 
            {
                var userInfo = await _userService.CreateUserAync(data);
                return Created("", userInfo);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
    }
}
