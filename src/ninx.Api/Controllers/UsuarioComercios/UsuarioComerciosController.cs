using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Communication;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuarioComercioController : NinxControllerBase
    {
        private readonly IUsuarioComercioService _usuarioComercioService;

        public UsuarioComercioController(IUsuarioComercioService usuarioComercioService)
        {
            _usuarioComercioService = usuarioComercioService;
        }

        [HttpPut]
        [ProducesResponseType(typeof(UsuarioComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Atualizar([FromBody] AtualizarUsuarioComercioRequest request)
        {
            var usuarioLogadoPermissao = GetPermissao();
            var result = await _usuarioComercioService.AtualizarAsync(request, usuarioLogadoPermissao);
            return Ok(result);
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Desativar([FromQuery] int usuarioId, [FromQuery] int comercioId)
        {
            await _usuarioComercioService.DesativarAsync(usuarioId, comercioId);
            return NoContent();
        }
    }
}

