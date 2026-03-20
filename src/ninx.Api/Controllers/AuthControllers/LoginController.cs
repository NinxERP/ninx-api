using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.Login;
using ninx.Communication.Request;

namespace ninx.Api.Controllers.AuthControllers
{
    [ApiController]
    [Route("api/auth/[controller]")]
    public class LoginController : ControllerBase
    {
            private readonly ILoginService _loginService;
            public LoginController(ILoginService loginService)
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
