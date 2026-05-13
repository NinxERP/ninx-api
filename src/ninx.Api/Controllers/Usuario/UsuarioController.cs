using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Communication;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsuarioController : NinxControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet("NoComercioId/{id}")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var usuarioIdLogado = GetUsuarioId();
            var usuario = await _usuarioService.GetById(id, usuarioIdLogado);
            return Ok(usuario);
        }

        [HttpGet("NoComercioId/All")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize)
        {
            var usuarioIdLogado = GetUsuarioId();
            var usuario = await _usuarioService.GetAll(usuarioIdLogado, pageNumber, pageSize);
            return Ok(usuario);
        }

        [HttpGet("All")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllByComercioId(int pageNumber, int pageSize)
        {
            var comercioId = GetComercioId();
            var usuario = await _usuarioService.GetAllByComercioId(comercioId, pageNumber, pageSize);
            return Ok(usuario);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAndComercioIdAsync(int id)
        {
            var comercioId = GetComercioId();
            var usuario = await _usuarioService.GetByIdAndComercioIdAsync(id, comercioId);
            return Ok(usuario);
        }


        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequest request)
        {
            var usuarioId = GetUsuarioId();
            var comercioId = GetComercioId();
            var permissao = GetPermissao();

            var usuario = await _usuarioService.CriarAsync(request, usuarioId, permissao, comercioId);
            return CreatedAtAction(nameof(GetById), new { id = usuario.UsuarioID }, usuario);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioRequest request)
        {
            var comercioId = GetComercioId();
            var usuario = await _usuarioService.AtualizarAsync(id, request, comercioId);
            return Ok(usuario);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            var comercioId = GetComercioId();
            await _usuarioService.DesativarAsync(id, comercioId);
            return NoContent();
        }
    }
}