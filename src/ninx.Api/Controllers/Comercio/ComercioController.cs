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
        /// Lista todos os comércios cadastrados na plataforma.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Comércios retornados com sucesso.</response>
        /// <response code="401">Apenas administradores globais podem utilizar esse endpoint.</response>
        [HttpGet("All")]
        [SwaggerOperation(Summary = "Listar todos os comércios", Description = "Retorna todos os comércios cadastrados na plataforma, sem restringir por vínculo. Restrito a administradores globais (Usuario.Admin == true).")]
        [ProducesResponseType(typeof(PaginatedResponse<ComercioResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
        {
            var usuarioIdLogado = GetUsuarioId();
            var result = await _comercioService.GetAll(request, usuarioIdLogado);
            return Ok(result);
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
        /// <response code="401">Apenas administradores globais podem criar comércios.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar comércio", Description = "Cadastra um novo comércio. Restrito a administradores globais (Usuario.Admin == true).")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Criar([FromBody] ComercioRequest request)
        {
            var usuarioIdLogado = GetUsuarioId();
            var result = await _comercioService.CriarAsync(request, usuarioIdLogado);
            return CreatedAtAction(nameof(GetById), new { id = result.ComercioID }, result);
        }

        /// <summary>
        /// Atualiza os dados de um comércio.
        /// </summary>
        /// <param name="comercioId">Identificador do comércio.</param>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Comércio atualizado com sucesso.</response>
        /// <response code="401">Sem vínculo com o comércio informado e não é administrador de plataforma.</response>
        /// <response code="404">Comércio não encontrado.</response>
        [HttpPut("{comercioId}")]
        [SwaggerOperation(Summary = "Atualizar comércio", Description = "Atualiza os dados do comércio informado. Restrito a quem tem cargo de peso Dono ou superior nesse comércio, ou administrador de plataforma (que pode atualizar qualquer comércio, independente do comércio ativo na sessão).")]
        [ProducesResponseType(typeof(ComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int comercioId, [FromBody] ComercioRequest request)
        {
            var usuarioID = GetUsuarioId();
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
