using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;

namespace ninx.Api.Controllers
{
    [ApiController]
    [Route("api/login/[controller]")]
    public class LoginController : ControllerBase
    {
            private readonly ILoginService _loginService;
            public LoginController(ILoginService loginService)
            {
                _loginService = loginService;
            }

        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _loginService.LoginAsync(request);
            return Ok(token);
        }
    }
}
