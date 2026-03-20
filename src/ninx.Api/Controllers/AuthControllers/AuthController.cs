using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.Login;
using ninx.Application.Services.Login;
using ninx.Communication.Request;
using ninx.Domain.Interfaces.Repositories.Usuario;
using ninx.Domain.Interfaces.Services.JwtToken;

namespace ninx.Api.Controllers.AuthControllers
{
    [ApiController]
    [Route("api/auth/[controller]")]
    public class AuthController : ControllerBase
    {
            private readonly ILoginService _loginService;
            public AuthController(ILoginService loginService)
            {
                _loginService = loginService;
            }

            [HttpPost("login")]
            public IActionResult Login([FromBody] LoginRequest request)
            {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

                var token = _loginService.LoginAsync(request);
                return Ok(new {Token = token});

            }
        }
}
