using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Cadastro e gestão dos comércios (tenants) do sistema.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Cadastro e gestão dos comércios (tenants) do sistema.")]
    public class ComercioController : NinxControllerBase
    {
        private readonly IComercioService _comercioService;

        public ComercioController(IComercioService comercioService)
        {
            _comercioService = comercioService;
        }

        /// <summary>
        /// Lista os comércios vinculados a um usuário.
        /// </summary>
        /// <param name="usuarioId">Identificador do usuário.</param>
        /// <response code="200">Comércios retornados com sucesso.</response>
        [HttpGet("usuario/{usuarioId}")]
        [SwaggerOperation(Summary = "Listar comércios por usuário", Description = "Retorna os comércios aos quais o usuário informado está vinculado.")]
        [ProducesResponseType(typeof(IEnumerable<ComercioResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var usuarioIdLogado = GetUsuarioId();
            var result = await _comercioService.GetByUsuarioId(usuarioId, usuarioIdLogado);
            return Ok(result);
        }

        /// <summary>
        /// Busca um comércio pelo identificador.
        /// </summary>
        /// <param name="comercioId">Identificador do comércio.</param>
        /// <response code="200">Comércio encontrado.</response>
        /// <response code="404">Comércio não encontrado.</response>
        [HttpGet("{comercioId}")]
        [SwaggerOperation(Summary = "Buscar comércio por id", Description = "Retorna um comércio pelo identificador, caso o usuário autenticado tenha acesso a ele.")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int comercioId)
        {
            var usuarioIdLogado = GetUsuarioId();
            var result = await _comercioService.GetByIdAsync(comercioId, usuarioIdLogado);
            return Ok(result);
        }

        /// <summary>
        /// Cadastra um novo comércio.
        /// </summary>
        /// <param name="request">Dados do comércio a ser criado.</param>
        /// <response code="201">Comércio criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar comércio", Description = "Cadastra um novo comércio.")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] ComercioRequest request)
        {
            var result = await _comercioService.CriarAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.ComercioID }, result);
        }

        /// <summary>
        /// Atualiza os dados do comércio autenticado.
        /// </summary>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Comércio atualizado com sucesso.</response>
        /// <response code="404">Comércio não encontrado.</response>
        [HttpPut("{comercioId}")]
        [SwaggerOperation(Summary = "Atualizar comércio", Description = "Atualiza os dados do comércio atualmente autenticado na sessão.")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar([FromBody] ComercioRequest request)
        {
            var usuarioID = GetUsuarioId();
            var comercioId = GetComercioId();
            var result = await _comercioService.AtualizarAsync(comercioId, usuarioID, request);
            return Ok(result);
        }

        /// <summary>
        /// Desativa um comércio.
        /// </summary>
        /// <param name="comercioId">Identificador do comércio.</param>
        /// <response code="204">Comércio desativado com sucesso.</response>
        /// <response code="404">Comércio não encontrado.</response>
        [HttpDelete("{comercioId}")]
        [SwaggerOperation(Summary = "Desativar comércio", Description = "Desativa (soft delete) um comércio.")]
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
