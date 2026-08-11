using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Registro, consulta e estorno de vendas, incluindo pagamentos fiado.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Registro, consulta e estorno de vendas, incluindo pagamentos fiado.")]
    public class VendaController : NinxControllerBase
    {
        private readonly IVendaService _vendaService;

        public VendaController(IVendaService vendaService)
        {
            _vendaService = vendaService;
        }

        /// <summary>
        /// Lista vendas do comércio autenticado a partir de filtros.
        /// </summary>
        /// <param name="request">Filtros de período, status, cliente, etc.</param>
        /// <response code="200">Vendas retornadas com sucesso.</response>
        /// <response code="404">Nenhuma venda encontrada para os filtros informados.</response>
        [HttpGet("filtro")]
        [SwaggerOperation(Summary = "Listar vendas filtradas", Description = "Retorna as vendas do comércio autenticado que atendem aos filtros informados.")]
        [ProducesResponseType(typeof(IEnumerable<VendaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVendasFiltro([FromQuery] FiltroRequest request)
        {
            var comercioId = GetComercioId();
            var result = await _vendaService.GetVendasFiltroAsync(request, comercioId);
            return Ok(result);
        }

        /// <summary>
        /// Lista as vendas registradas por um usuário (vendedor).
        /// </summary>
        /// <param name="usuarioId">Identificador do usuário vendedor.</param>
        /// <response code="200">Vendas retornadas com sucesso.</response>
        /// <response code="404">Nenhuma venda encontrada para o usuário informado.</response>
        [HttpGet("usuario/{usuarioId}")]
        [SwaggerOperation(Summary = "Listar vendas por usuário", Description = "Retorna as vendas do comércio autenticado registradas pelo usuário informado.")]
        [ProducesResponseType(typeof(IEnumerable<VendaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var comercioId = GetComercioId();
            var result = await _vendaService.GetByUsuarioIdAsync(usuarioId, comercioId);
            return Ok(result);
        }

        /// <summary>
        /// Lista as vendas de um cliente.
        /// </summary>
        /// <param name="clienteId">Identificador do cliente.</param>
        /// <response code="200">Vendas retornadas com sucesso.</response>
        /// <response code="404">Nenhuma venda encontrada para o cliente informado.</response>
        [HttpGet("cliente/{clienteId}")]
        [SwaggerOperation(Summary = "Listar vendas por cliente", Description = "Retorna as vendas do comércio autenticado associadas ao cliente informado.")]
        [ProducesResponseType(typeof(IEnumerable<VendaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByClienteId(int clienteId)
        {
            var comercioId = GetComercioId();
            var result = await _vendaService.GetByClienteIdAsync(clienteId, comercioId);
            return Ok(result);
        }

        /// <summary>
        /// Busca uma venda pelo identificador.
        /// </summary>
        /// <param name="vendaId">Identificador da venda.</param>
        /// <response code="200">Venda encontrada.</response>
        /// <response code="404">Venda não encontrada no comércio autenticado.</response>
        [HttpGet("{vendaId}")]
        [SwaggerOperation(Summary = "Buscar venda por id", Description = "Retorna uma venda do comércio autenticado pelo identificador.")]
        [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByVendaId(int vendaId)
        {
            var comercioId = GetComercioId();
            var result = await _vendaService.GetByVendaIdAsync(vendaId, comercioId);
            return Ok(result);
        }

        /// <summary>
        /// Registra uma nova venda.
        /// </summary>
        /// <param name="request">Itens, forma de pagamento e demais dados da venda.</param>
        /// <response code="201">Venda registrada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Registrar venda", Description = "Registra uma nova venda para o comércio e usuário autenticados.")]
        [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarVendaRequest request)
        {
            request.ComercioID = GetComercioId();
            request.UsuarioID = GetUsuarioId();
            var result = await _vendaService.CriarAsync(request);
            return CreatedAtAction(nameof(GetByVendaId), new { vendaId = result.VendaID }, result);
        }

        /// <summary>
        /// Estorna uma venda.
        /// </summary>
        /// <param name="vendaId">Identificador da venda a ser estornada.</param>
        /// <response code="204">Venda estornada com sucesso.</response>
        /// <response code="400">A venda não pode ser estornada.</response>
        /// <response code="404">Venda não encontrada no comércio autenticado.</response>
        [HttpPost("{vendaId}/estorno")]
        [SwaggerOperation(Summary = "Estornar venda", Description = "Estorna uma venda existente, revertendo estoque e pagamento quando aplicável.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Estornar(int vendaId)
        {
            var usuarioId = GetUsuarioId();
            await _vendaService.EstornarAsync(vendaId, usuarioId);
            return NoContent();
        }

        /// <summary>
        /// Registra o pagamento de uma venda fiado.
        /// </summary>
        /// <param name="vendaId">Identificador da venda fiado.</param>
        /// <param name="request">Valor pago e forma de pagamento.</param>
        /// <response code="200">Pagamento registrado com sucesso.</response>
        /// <response code="400">Dados de pagamento inválidos.</response>
        /// <response code="404">Venda não encontrada no comércio autenticado.</response>
        [HttpPost("{vendaId}/pagamento-fiado")]
        [SwaggerOperation(Summary = "Receber pagamento de venda fiado", Description = "Registra o recebimento de um pagamento referente a uma venda fiado específica.")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReceberPagamentoFiado(int vendaId, [FromBody] ReceberPagamentoFiadoRequest request)
        {
            var usuarioId = GetUsuarioId();

            var response = await _vendaService.ReceberPagamentoFiadoAsync(
                vendaId,
                usuarioId,
                request.ValorPago,
                request.FormaPagamento);

            return Ok(response);
        }

        /// <summary>
        /// Registra o pagamento geral do fiado de um cliente.
        /// </summary>
        /// <param name="clienteId">Identificador do cliente.</param>
        /// <param name="request">Valor pago e forma de pagamento.</param>
        /// <response code="200">Pagamento registrado com sucesso.</response>
        /// <response code="400">Dados de pagamento inválidos.</response>
        /// <response code="404">Cliente não encontrado no comércio autenticado.</response>
        [HttpPost("cliente/{clienteId}/pagamento-geral-fiado")]
        [SwaggerOperation(Summary = "Receber pagamento geral do fiado", Description = "Registra o recebimento de um pagamento distribuído entre as vendas fiado em aberto de um cliente.")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReceberPagamentoGeralFiado(int clienteId, [FromBody] ReceberPagamentoFiadoRequest request)
        {
            var usuarioId = GetUsuarioId();

            Guid documentoGuid = await _vendaService.ReceberPagamentoGeralFiadoAsync(
                clienteId,
                usuarioId,
                request.ValorPago,
                request.FormaPagamento);

            return Ok(documentoGuid);
        }
    }
}
