using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Interfaces.Services;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _usuarioService.GetByIdAsync(id);
            if (usuario is null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequest request)
        {
            var usuario = await _usuarioService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = usuario.UsuarioID }, usuario);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioRequest request)
        {
            var usuario = await _usuarioService.AtualizarAsync(id, request);
            return Ok(usuario);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            await _usuarioService.DesativarAsync(id);
            return NoContent();
        }
    }
}