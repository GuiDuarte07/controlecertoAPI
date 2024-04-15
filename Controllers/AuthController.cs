using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        [AllowAnonymous]
        [HttpPost()]
        public ActionResult Authenticate([FromBody] LoginRequest data)
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
