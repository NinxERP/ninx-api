using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Autenticação de usuários e emissão de token JWT.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Autenticação de usuários e emissão de token JWT.")]
    public class LoginController : NinxControllerBase
    {
            private readonly ILoginService _loginService;
            public LoginController(ILoginService loginService)
            {
                _loginService = loginService;
            }

        /// <summary>
        /// Autentica um usuário e retorna o token JWT.
        /// </summary>
        /// <remarks>Endpoint público, não exige autenticação.</remarks>
        /// <param name="request">E-mail e senha do usuário.</param>
        /// <response code="200">Token JWT emitido com sucesso.</response>
        /// <response code="400">Credenciais inválidas.</response>
        [HttpPost]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Login", Description = "Valida as credenciais do usuário e retorna o token JWT usado nos demais endpoints.")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _loginService.LoginAsync(request);
            return Ok(token);
        }
    }
}
