using Finantech.DTOs.Events;
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
        private readonly IBus _bus;
        private readonly ICacheService _cacheService;
        private readonly IDistributedCache _cache;
        public UserController(IUserService userService,IBus bus, ICacheService cacheService, IDistributedCache cache)
        {
            _userService = userService;
            _bus = bus;
            _cacheService = cacheService;
            _cache = cache;
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

        [AllowAnonymous]
        [HttpGet("TesteCache")]
        public async Task<IActionResult> TesteCache()
        {
            var token = RandomGenerate.Generate32BytesToken();
            await _cacheService.SetConfirmEmailTokenAsync("guilhduart.abr@gmail.com", token);

            var tokenValue = await _cacheService.GetConfirmEmailTokenAsync(token);

            return Ok(tokenValue);
        }

        /*[AllowAnonymous]
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
        }*/
    }
}
