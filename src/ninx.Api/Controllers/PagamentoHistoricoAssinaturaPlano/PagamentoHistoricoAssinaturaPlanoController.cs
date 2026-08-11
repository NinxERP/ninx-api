using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Api.Controllers;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.API.Controllers
{
    /// <summary>
    /// Registro de pagamentos de assinatura de plano.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Registro de pagamentos de assinatura de plano.")]
    public class PagamentoAssinaturaController : NinxControllerBase
    {
        private readonly IPagamentoHistoricoAssinaturaPlanoService _pagamentoService;

        public PagamentoAssinaturaController(IPagamentoHistoricoAssinaturaPlanoService pagamentoService)
        {
            _pagamentoService = pagamentoService;
        }

        /// <summary>
        /// Registra o pagamento da assinatura do comércio autenticado.
        /// </summary>
        /// <param name="request">Dados do pagamento a ser registrado.</param>
        /// <response code="204">Pagamento registrado com sucesso.</response>
        /// <response code="400">Dados de pagamento inválidos.</response>
        /// <response code="403">Usuário autenticado não tem permissão para registrar o pagamento.</response>
        /// <response code="404">Comércio ou assinatura não encontrados.</response>
        [HttpPost("registrar")]
        [SwaggerOperation(Summary = "Registrar pagamento de assinatura", Description = "Registra o pagamento da assinatura de plano do comércio autenticado.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarPagamento([FromBody] PagamentoHistoricoAssinaturaPlanoRequest request)
        {
            var permissao = GetPermissao();
            request.ComercioId = GetComercioId();

            await _pagamentoService.RegistrarPagamentos(request, permissao);

            return NoContent();
        }
    }
}
