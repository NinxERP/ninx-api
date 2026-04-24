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
    public class ClienteController : NinxControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var clientes = await _clienteService.GetAll();
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var cliente = await _clienteService.GetByIdAsync(id);
            return Ok(cliente);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] ClienteRequest request)
        {
            var cliente = await _clienteService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = cliente.ClienteID }, cliente);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ClienteRequest request)
        {
            var usuarioId = GetUsuarioId();
            var cliente = await _clienteService.AtualizarAsync(id, usuarioId, request);
            return Ok(cliente);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            await _clienteService.DesativarAsync(id);
            return NoContent();
        }
    }
}