using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RelatorioController : NinxControllerBase
    {
        private readonly IRelatorioService _relatorioService;

        public RelatorioController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(RelatorioDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDashboard([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetDashboardAsync(comercioId, request);
            return Ok(result);
        }

        [HttpGet("curva-abc")]
        [ProducesResponseType(typeof(List<ProdutoCurvaAbcResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCurvaAbc([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetCurvaAbcAsync(comercioId, request);
            return Ok(result);
        }

        [HttpGet("margem")]
        [ProducesResponseType(typeof(RelatorioMargemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMargem([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetMargemAsync(comercioId, request);
            return Ok(result);
        }

        [HttpGet("aging-recebiveis")]
        [ProducesResponseType(typeof(RelatorioAgingResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgingRecebiveis()
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetAgingRecebiveisAsync(comercioId);
            return Ok(result);
        }

        [HttpGet("desempenho-vendedores")]
        [ProducesResponseType(typeof(List<VendedorDesempenhoResumo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDesempenhoVendedores([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetDesempenhoVendedoresAsync(comercioId, request);
            return Ok(result);
        }

        [HttpGet("pico-vendas")]
        [ProducesResponseType(typeof(RelatorioPicoVendasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPicoVendas([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetPicoVendasAsync(comercioId, request);
            return Ok(result);
        }

        [HttpGet("clientes-inativos")]
        [ProducesResponseType(typeof(List<ClienteInativoResumo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClientesInativos([FromQuery] RelatorioClientesInativosRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetClientesInativosAsync(comercioId, request);
            return Ok(result);
        }

        [HttpGet("limite-credito")]
        [ProducesResponseType(typeof(List<ClienteLimiteCreditoResumo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsoLimiteCredito()
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetUsoLimiteCreditoAsync(comercioId);
            return Ok(result);
        }

        [HttpGet("giro-estoque")]
        [ProducesResponseType(typeof(RelatorioGiroEstoqueResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGiroEstoque([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetGiroEstoqueAsync(comercioId, request);
            return Ok(result);
        }

        [HttpGet("produtos-vencendo")]
        [ProducesResponseType(typeof(List<ProdutoVencendoResumo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetProdutosVencendo([FromQuery] RelatorioProdutosVencendoRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetProdutosVencendoAsync(comercioId, request);
            return Ok(result);
        }

        [HttpGet("comparativo-comercios")]
        [ProducesResponseType(typeof(RelatorioComparativoComerciosResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetComparativoComercios([FromQuery] RelatorioDashboardRequest request)
        {
            var usuarioId = GetUsuarioId();
            var result = await _relatorioService.GetComparativoComerciosAsync(usuarioId, request);
            return Ok(result);
        }
    }
}
