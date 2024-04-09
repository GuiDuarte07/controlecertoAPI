using Finantech.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost()]
        public async Task<ActionResult> Authenticate([FromBody] LoginRequest data)
        {

            var user = await _authService.Authenticate(data.Email, data.Password);

            if (!user)
            {
                return Unauthorized("Credenciais inválidas.");
            }

            var token = _authService.GenerateToken(user);

            return Ok(token);

            
        }
    }
}
}
