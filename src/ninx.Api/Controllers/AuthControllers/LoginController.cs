using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.Login;
using ninx.Communication.Request.Login;
using ninx.Communication.Response;
using ninx.Communication.Response.Login;
using ninx.Communication.Response.UsuarioComercio;

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
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _loginService.LoginAsync(request);
            return Ok(token);
        }
    }
}
