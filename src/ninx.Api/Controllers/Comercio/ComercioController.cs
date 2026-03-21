using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.Comercio;
using ninx.Communication.Request.Comercio;
using ninx.Communication.Response;
using ninx.Communication.Response.Comercio;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ComercioController : ControllerBase
    {
        private readonly IComercioService _comercioService;

        public ComercioController(IComercioService comercioService)
        {
            _comercioService = comercioService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ComercioResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _comercioService.GetAll();
            return Ok(result);
        }

        [HttpGet("usuario/{usuarioId}")]
        [ProducesResponseType(typeof(IEnumerable<ComercioResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var result = await _comercioService.GetByUsuarioId(usuarioId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _comercioService.GetByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] ComercioRequest request)
        {
            var result = await _comercioService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ComercioID }, result);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ComercioRequest request)
        {
            var result = await _comercioService.AtualizarAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            await _comercioService.DesativarAsync(id);
            return NoContent();
        }
    }
}