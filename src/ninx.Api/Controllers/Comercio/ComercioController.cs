using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;


namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ComercioController : NinxControllerBase
    {
        private readonly IComercioService _comercioService;

        public ComercioController(IComercioService comercioService)
        {
            _comercioService = comercioService;
        }

        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(IEnumerable<ComercioResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var result = await _comercioService.GetByUsuarioId(usuarioId);
            return Ok(result);
        }

        [HttpGet("{comercioId}")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int comercioId)
        {
            var result = await _comercioService.GetByIdAsync(comercioId);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] ComercioRequest request)
        {
            var result = await _comercioService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ComercioID }, result);
        }

        [HttpPut("{comercioId}")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar([FromBody] ComercioRequest request)
        {
            var usuarioID = GetUsuarioId();
            var comercioId = GetComercioId();
            var result = await _comercioService.AtualizarAsync(comercioId, usuarioID, request);
            return Ok(result);
        }

        [HttpDelete("{comercioId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int comercioId)
        {
            var usuarioIdLogado = GetUsuarioId();
            await _comercioService.DesativarAsync(comercioId, usuarioIdLogado);
            return NoContent();
        }
    }
}