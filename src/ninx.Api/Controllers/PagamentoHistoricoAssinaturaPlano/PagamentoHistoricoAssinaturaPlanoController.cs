using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Api.Controllers;
using ninx.Application.Services;
using ninx.Communication;

namespace ninx.API.Controllers
{
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class PagamentoAssinaturaController : NinxControllerBase
    {
        private readonly IPagamentoHistoricoAssinaturaPlanoService _pagamentoService;

        public PagamentoAssinaturaController(IPagamentoHistoricoAssinaturaPlanoService pagamentoService)
        {
            _pagamentoService = pagamentoService;
        }

        [HttpPost("registrar")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegistrarPagamento([FromBody] PagamentoHistoricoAssinaturaPlanoRequest request)
        {
            var permissao = GetPermissao();

            await _pagamentoService.RegistrarPagamentos(request, permissao);

            return NoContent();
        }
    }
}