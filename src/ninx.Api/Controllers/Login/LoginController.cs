using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Communication;

namespace ninx.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : NinxControllerBase
    {
            private readonly ILoginService _loginService;
            public LoginController(ILoginService loginService)
            {
                _loginService = loginService;
            }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _loginService.LoginAsync(request);
            return Ok(token);
        }
    }
}
