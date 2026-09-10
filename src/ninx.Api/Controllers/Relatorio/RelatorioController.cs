using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Relatórios gerenciais do comércio (dashboard, curva ABC, margem, entre outros).
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Relatórios gerenciais do comércio (dashboard, curva ABC, margem, entre outros).")]
    public class RelatorioController : NinxControllerBase
    {
        private readonly IRelatorioService _relatorioService;

        public RelatorioController(IRelatorioService relatorioService)
        {
            _relatorioService = relatorioService;
        }

        /// <summary>
        /// Retorna os indicadores do dashboard do comércio.
        /// </summary>
        /// <param name="request">Período e demais filtros do relatório.</param>
        /// <response code="200">Dashboard retornado com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("dashboard")]
        [SwaggerOperation(Summary = "Dashboard do comércio", Description = "Retorna os indicadores gerenciais consolidados do comércio autenticado.")]
        [ProducesResponseType(typeof(RelatorioDashboardResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDashboard([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetDashboardAsync(comercioId, request);
            return Ok(result);
        }

        /// <summary>
        /// Retorna a curva ABC de produtos.
        /// </summary>
        /// <param name="request">Período e demais filtros do relatório.</param>
        /// <response code="200">Curva ABC retornada com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("curva-abc")]
        [SwaggerOperation(Summary = "Curva ABC de produtos", Description = "Classifica os produtos do comércio autenticado em faixas A/B/C conforme representatividade nas vendas.")]
        [ProducesResponseType(typeof(List<ProdutoCurvaAbcResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCurvaAbc([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetCurvaAbcAsync(comercioId, request);
            return Ok(result);
        }

        /// <summary>
        /// Retorna a margem de lucro do comércio no período.
        /// </summary>
        /// <param name="request">Período e demais filtros do relatório.</param>
        /// <response code="200">Margem retornada com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("margem")]
        [SwaggerOperation(Summary = "Margem de lucro", Description = "Retorna a margem de lucro do comércio autenticado no período informado.")]
        [ProducesResponseType(typeof(RelatorioMargemResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetMargem([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetMargemAsync(comercioId, request);
            return Ok(result);
        }

        /// <summary>
        /// Retorna o aging dos recebíveis (vendas fiado em aberto).
        /// </summary>
        /// <response code="200">Aging de recebíveis retornado com sucesso.</response>
        [HttpGet("aging-recebiveis")]
        [SwaggerOperation(Summary = "Aging de recebíveis", Description = "Retorna a distribuição das vendas fiado em aberto do comércio autenticado por faixa de atraso.")]
        [ProducesResponseType(typeof(RelatorioAgingResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAgingRecebiveis()
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetAgingRecebiveisAsync(comercioId);
            return Ok(result);
        }

        /// <summary>
        /// Retorna o desempenho de vendas por vendedor.
        /// </summary>
        /// <param name="request">Período e demais filtros do relatório.</param>
        /// <response code="200">Desempenho de vendedores retornado com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("desempenho-vendedores")]
        [SwaggerOperation(Summary = "Desempenho de vendedores", Description = "Retorna o resumo de desempenho de vendas por vendedor do comércio autenticado.")]
        [ProducesResponseType(typeof(List<VendedorDesempenhoResumo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDesempenhoVendedores([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetDesempenhoVendedoresAsync(comercioId, request);
            return Ok(result);
        }

        /// <summary>
        /// Retorna o pico de vendas por dia da semana e horário.
        /// </summary>
        /// <param name="request">Período e demais filtros do relatório.</param>
        /// <response code="200">Pico de vendas retornado com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("pico-vendas")]
        [SwaggerOperation(Summary = "Pico de vendas", Description = "Retorna a distribuição de vendas do comércio autenticado por dia da semana e horário, para identificar os picos de movimento.")]
        [ProducesResponseType(typeof(RelatorioPicoVendasResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPicoVendas([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetPicoVendasAsync(comercioId, request);
            return Ok(result);
        }

        /// <summary>
        /// Lista os clientes inativos do comércio.
        /// </summary>
        /// <param name="request">Filtros do relatório, como período sem compras.</param>
        /// <response code="200">Clientes inativos retornados com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("clientes-inativos")]
        [SwaggerOperation(Summary = "Clientes inativos", Description = "Lista os clientes do comércio autenticado sem compras recentes, conforme os filtros informados.")]
        [ProducesResponseType(typeof(List<ClienteInativoResumo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetClientesInativos([FromQuery] RelatorioClientesInativosRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetClientesInativosAsync(comercioId, request);
            return Ok(result);
        }

        /// <summary>
        /// Retorna o uso do limite de crédito dos clientes.
        /// </summary>
        /// <response code="200">Uso de limite de crédito retornado com sucesso.</response>
        [HttpGet("limite-credito")]
        [SwaggerOperation(Summary = "Uso do limite de crédito", Description = "Retorna o percentual de uso do limite de crédito (fiado) de cada cliente do comércio autenticado.")]
        [ProducesResponseType(typeof(List<ClienteLimiteCreditoResumo>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsoLimiteCredito()
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetUsoLimiteCreditoAsync(comercioId);
            return Ok(result);
        }

        /// <summary>
        /// Retorna o giro de estoque dos produtos.
        /// </summary>
        /// <param name="request">Período e demais filtros do relatório.</param>
        /// <response code="200">Giro de estoque retornado com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("giro-estoque")]
        [SwaggerOperation(Summary = "Giro de estoque", Description = "Retorna o giro de estoque dos produtos do comércio autenticado no período informado.")]
        [ProducesResponseType(typeof(RelatorioGiroEstoqueResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetGiroEstoque([FromQuery] RelatorioDashboardRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetGiroEstoqueAsync(comercioId, request);
            return Ok(result);
        }

        /// <summary>
        /// Lista os produtos próximos do vencimento.
        /// </summary>
        /// <param name="request">Filtros do relatório, como janela de dias até o vencimento.</param>
        /// <response code="200">Produtos vencendo retornados com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("produtos-vencendo")]
        [SwaggerOperation(Summary = "Produtos vencendo", Description = "Lista os produtos do comércio autenticado próximos da data de validade.")]
        [ProducesResponseType(typeof(List<ProdutoVencendoResumo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetProdutosVencendo([FromQuery] RelatorioProdutosVencendoRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _relatorioService.GetProdutosVencendoAsync(comercioId, request);
            return Ok(result);
        }

        /// <summary>
        /// Compara o desempenho entre os comércios do usuário autenticado.
        /// </summary>
        /// <param name="request">Período e demais filtros do relatório.</param>
        /// <response code="200">Comparativo entre comércios retornado com sucesso.</response>
        /// <response code="400">Filtros inválidos.</response>
        [HttpGet("comparativo-comercios")]
        [SwaggerOperation(Summary = "Comparativo entre comércios", Description = "Compara indicadores de desempenho entre os comércios vinculados ao usuário autenticado.")]
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
