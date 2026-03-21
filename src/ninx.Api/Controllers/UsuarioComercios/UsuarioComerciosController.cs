using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Communication.Request.UsuarioComercios;
using ninx.Communication.Response;
using ninx.Communication.Response.UsuarioComercio;
using ninx.Domain.Interfaces.Services;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuarioComercioController : ControllerBase
    {
        private readonly IUsuarioComercioService _usuarioComercioService;

        public UsuarioComercioController(IUsuarioComercioService usuarioComercioService)
        {
            _usuarioComercioService = usuarioComercioService;
        }

        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioComercioResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var result = await _usuarioComercioService.GetByUsuarioIdAsync(usuarioId);
            return Ok(result);
        }

        [HttpGet("comercio/{comercioId}")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioComercioResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByComercioId(int comercioId)
        {
            var result = await _usuarioComercioService.GetByComercioIdAsync(comercioId);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UsuarioComercioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarUsuarioComercioRequest request)
        {
            var result = await _usuarioComercioService.CriarAsync(request);
            return Created(string.Empty, result);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar([FromQuery] int usuarioId, [FromQuery] int comercioId)
        {
            await _usuarioComercioService.DesativarAsync(usuarioId, comercioId);
            return NoContent();
        }
    }
}