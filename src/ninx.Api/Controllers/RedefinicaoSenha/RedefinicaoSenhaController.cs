using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Redefinição de senha self-service via código enviado por e-mail.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Redefinição de senha self-service via código enviado por e-mail.")]
    public class RedefinicaoSenhaController : NinxControllerBase
    {
        private readonly IRedefinicaoSenhaService _redefinicaoSenhaService;

        public RedefinicaoSenhaController(IRedefinicaoSenhaService redefinicaoSenhaService)
        {
            _redefinicaoSenhaService = redefinicaoSenhaService;
        }

        /// <summary>
        /// Solicita um código de redefinição de senha por e-mail.
        /// </summary>
        /// <remarks>Endpoint público, não exige autenticação.</remarks>
        /// <param name="request">E-mail do usuário.</param>
        /// <response code="200">Solicitação processada (sempre retorna sucesso, mesmo se o e-mail não estiver cadastrado).</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost("Solicitar")]
        [AllowAnonymous]
        [EnableRateLimiting("RedefinicaoSenhaSolicitar")]
        [SwaggerOperation(Summary = "Solicitar código de redefinição de senha", Description = "Envia um código de 6 dígitos por e-mail, caso o e-mail informado esteja cadastrado. Sempre retorna sucesso, para não revelar se o e-mail existe na base.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Solicitar([FromBody] SolicitarRedefinicaoSenhaRequest request)
        {
             await _redefinicaoSenhaService.SolicitarAsync(request);
            return Ok(new { mensagem = "Se o e-mail informado estiver cadastrado, um código de redefinição foi enviado." });
        }

        /// <summary>
        /// Confirma o código recebido por e-mail e define uma nova senha.
        /// </summary>
        /// <remarks>Endpoint público, não exige autenticação.</remarks>
        /// <param name="request">E-mail, código recebido e nova senha.</param>
        /// <response code="200">Senha redefinida com sucesso.</response>
        /// <response code="400">Código inválido, expirado ou dados inválidos.</response>
        [HttpPost("Confirmar")]
        [AllowAnonymous]
        [EnableRateLimiting("RedefinicaoSenhaConfirmar")]
        [SwaggerOperation(Summary = "Confirmar redefinição de senha", Description = "Valida o código de 6 dígitos enviado por e-mail e, se correto, define a nova senha informada.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Confirmar([FromBody] ConfirmarRedefinicaoSenhaRequest request)
        {
            await _redefinicaoSenhaService.ConfirmarAsync(request);
            return Ok(new { mensagem = "Senha redefinida com sucesso." });
        }
    }
}
