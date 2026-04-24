using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;


namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VendaController : NinxControllerBase
    {
        private readonly IVendaService _vendaService;

        public VendaController(IVendaService vendaService)
        {
            _vendaService = vendaService;
        }

        [HttpGet("filtro")]
        [ProducesResponseType(typeof(IEnumerable<VendaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetVendasFiltro([FromQuery] FiltroRequest request)
        {
            var result = await _vendaService.GetVendasFiltroAsync(request);
            return Ok(result);
        }

        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(IEnumerable<VendaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var result = await _vendaService.GetByUsuarioIdAsync(usuarioId);
            return Ok(result);
        }

        [HttpGet("{vendaId}")]
        [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByVendaId(int vendaId)
        {
            var result = await _vendaService.GetByVendaIdAsync(vendaId);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(VendaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarVendaRequest request)
        {
            var result = await _vendaService.CriarAsync(request);
            return CreatedAtAction(nameof(GetByVendaId), new { vendaId = result.VendaID }, result);
        }

        [HttpPost("{vendaId}/estorno")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Estornar(int vendaId)
        {
            var usuarioId = GetUsuarioId();
            await _vendaService.EstornarAsync(vendaId, usuarioId);
            return NoContent();
        }
    }
}
