using Finantech.DTOs.Auth;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public ActionResult Authenticate([FromBody] AuthRequest data)
        {

            var authInfo = _authService.Authenticate(data.Email, data.Password);

            if (authInfo == null)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            return Ok(authInfo);

            
        }
    }
}
