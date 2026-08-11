using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Consulta e movimentação de estoque dos produtos.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Consulta e movimentação de estoque dos produtos.")]
    public class EstoqueController : NinxControllerBase
    {
        private readonly IEstoqueService _estoqueService;

        public EstoqueController(IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }

        /// <summary>
        /// Lista os registros de estoque do comércio autenticado.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Registros de estoque retornados com sucesso.</response>
        [HttpGet]
        [Route("All")]
        [SwaggerOperation(Summary = "Listar estoque", Description = "Retorna os registros de estoque do comércio autenticado.")]
        [ProducesResponseType(typeof(IEnumerable<EstoqueResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(PaginationRequest request)
        {
            var comercioId = GetComercioId();
            var estoques = await _estoqueService.GetAllByComercioIdAsync(comercioId, request);
            return Ok(estoques);
        }

        /// <summary>
        /// Busca um registro de estoque pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do registro de estoque.</param>
        /// <response code="200">Registro de estoque encontrado.</response>
        /// <response code="404">Registro de estoque não encontrado no comércio autenticado.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Buscar estoque por id", Description = "Retorna um registro de estoque do comércio autenticado pelo identificador.")]
        [ProducesResponseType(typeof(EstoqueResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var comercioId = GetComercioId();
            var estoque = await _estoqueService.GetByIdAsync(id, comercioId);
            return Ok(estoque);
        }

        /// <summary>
        /// Cria um novo registro de estoque.
        /// </summary>
        /// <param name="request">Dados do registro de estoque a ser criado.</param>
        /// <response code="201">Registro de estoque criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar estoque", Description = "Cria um novo registro de estoque para o comércio autenticado.")]
        [ProducesResponseType(typeof(EstoqueResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] EstoqueRequest request)
        {
            var comercioId = GetComercioId();
            var estoque = await _estoqueService.CreateAsync(request, comercioId);
            return CreatedAtAction(nameof(GetById), new { id = estoque.EstoqueID }, estoque);
        }

        /// <summary>
        /// Atualiza um registro de estoque existente.
        /// </summary>
        /// <param name="id">Identificador do registro de estoque.</param>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Registro de estoque atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Registro de estoque não encontrado no comércio autenticado.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar estoque", Description = "Atualiza um registro de estoque do comércio autenticado.")]
        [ProducesResponseType(typeof(EstoqueResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] EstoqueRequest request)
        {
            var comercioId = GetComercioId();
            var estoque = await _estoqueService.UpdateAsync(id, request, comercioId);
            return Ok(estoque);
        }

        /// <summary>
        /// Remove um registro de estoque.
        /// </summary>
        /// <param name="id">Identificador do registro de estoque.</param>
        /// <response code="204">Registro de estoque removido com sucesso.</response>
        /// <response code="404">Registro de estoque não encontrado no comércio autenticado.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Remover estoque", Description = "Remove um registro de estoque do comércio autenticado.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var comercioId = GetComercioId();
            await _estoqueService.DeleteAsync(id, comercioId);
            return NoContent();
        }
    }
}
