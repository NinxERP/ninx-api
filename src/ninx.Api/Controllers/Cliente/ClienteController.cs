using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Cadastro e consulta de clientes do comércio.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Cadastro e consulta de clientes do comércio.")]
    public class ClienteController : NinxControllerBase
    {
        private readonly IClienteService _clienteService;

        public ClienteController(IClienteService clienteService)
        {
            _clienteService = clienteService;
        }

        /// <summary>
        /// Lista os clientes do comércio autenticado.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Clientes retornados com sucesso.</response>
        [HttpGet]
        [Route("All")]
        [SwaggerOperation(Summary = "Listar clientes", Description = "Retorna os clientes do comércio autenticado.")]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
        {
            var comercioId = GetComercioId();
            var clientes = await _clienteService.GetAllByComercioId(comercioId, request);
            return Ok(clientes);
        }

        /// <summary>
        /// Busca clientes pelo nome.
        /// </summary>
        /// <param name="nome">Termo de busca pelo nome do cliente.</param>
        /// <response code="200">Clientes encontrados.</response>
        [HttpGet]
        [Route("nome/{nome}")]
        [SwaggerOperation(Summary = "Buscar clientes por nome", Description = "Retorna os clientes do comércio autenticado cujo nome corresponde ao termo informado.")]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetByNomeAsync(string nome)
        {
            var comercioId = GetComercioId();
            var clientes = await _clienteService.GetByNomeAsync(nome, comercioId);
            return Ok(clientes);
        }

        /// <summary>
        /// Busca um cliente pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do cliente.</param>
        /// <response code="200">Cliente encontrado.</response>
        /// <response code="404">Cliente não encontrado no comércio autenticado.</response>
        [HttpGet("id/{id}")]
        [SwaggerOperation(Summary = "Buscar cliente por id", Description = "Retorna um cliente do comércio autenticado pelo identificador.")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var comercioId = GetComercioId();
            var cliente = await _clienteService.GetByIdAsync(id, comercioId);
            return Ok(cliente);
        }

        /// <summary>
        /// Cadastra um novo cliente.
        /// </summary>
        /// <param name="request">Dados do cliente a ser criado.</param>
        /// <response code="201">Cliente criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar cliente", Description = "Cadastra um novo cliente para o comércio autenticado.")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] ClienteRequest request)
        {
            var comercioId = GetComercioId();
            var cliente = await _clienteService.CriarAsync(request, comercioId);
            return CreatedAtAction(nameof(GetById), new { id = cliente.ClienteID }, cliente);
        }

        /// <summary>
        /// Atualiza os dados de um cliente existente.
        /// </summary>
        /// <param name="id">Identificador do cliente.</param>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Cliente atualizado com sucesso.</response>
        /// <response code="404">Cliente não encontrado no comércio autenticado.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar cliente", Description = "Atualiza os dados de um cliente do comércio autenticado.")]
        [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] ClienteRequest request)
        {
            var usuarioId = GetUsuarioId();
            var comercioId = GetComercioId();
            var cliente = await _clienteService.AtualizarAsync(id, usuarioId, request, comercioId);
            return Ok(cliente);
        }

        /// <summary>
        /// Desativa um cliente.
        /// </summary>
        /// <param name="id">Identificador do cliente.</param>
        /// <response code="204">Cliente desativado com sucesso.</response>
        /// <response code="404">Cliente não encontrado no comércio autenticado.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Desativar cliente", Description = "Desativa (soft delete) um cliente do comércio autenticado.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            var comercioId = GetComercioId();
            await _clienteService.DesativarAsync(id, comercioId);
            return NoContent();
        }
    }
}
