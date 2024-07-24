using Finantech.DTOs.Events;
using Finantech.DTOs.User;
using Finantech.Extensions;
using Finantech.Services.Interfaces;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IBus _bus;
        public UserController(IUserService userService,IBus bus)
        {
            _userService = userService;
            _bus = bus;
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
        [HttpGet("messege/{message}")]
        public IActionResult NewConsoleMessage(string message)
        {
            _bus.Publish(new ConsoleMessageEvent(message));

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("sendEmail")]
        public IActionResult NewConsoleMessage([FromBody] EmailEvent email)
        {
            _bus.Publish(new EmailEvent(email.Emails, email.Subject, email.Body) );

            return Ok();
        }
    }
}
