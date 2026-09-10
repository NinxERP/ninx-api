using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Communication.Response;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Categorias usadas para organizar os produtos do comércio.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Categorias usadas para organizar os produtos do comércio.")]
    public class CategoriaProdutoController : NinxControllerBase
    {
        private readonly ICategoriaProdutoService _categoriaProdutoService;

        public CategoriaProdutoController(ICategoriaProdutoService categoriaProdutoService)
        {
            _categoriaProdutoService = categoriaProdutoService;
        }

        /// <summary>
        /// Lista as categorias de produto do comércio autenticado.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Categorias retornadas com sucesso.</response>
        [HttpGet]
        [Route("All")]
        [SwaggerOperation(Summary = "Listar categorias de produto", Description = "Retorna as categorias de produto do comércio autenticado.")]
        [ProducesResponseType(typeof(PaginatedResponse<CategoriaProdutoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
        {
            var comercioId = GetComercioId();
            var categorias = await _categoriaProdutoService.GetAllAsync(comercioId, request);
            return Ok(categorias);
        }

        /// <summary>
        /// Busca uma categoria de produto pelo identificador.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <response code="200">Categoria encontrada.</response>
        /// <response code="404">Categoria não encontrada no comércio autenticado.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Buscar categoria por id", Description = "Retorna uma categoria de produto do comércio autenticado pelo identificador.")]
        [ProducesResponseType(typeof(CategoriaProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var comercioId = GetComercioId();
            var categoria = await _categoriaProdutoService.GetByIdAsync(id, comercioId);
            return Ok(categoria);
        }

        /// <summary>
        /// Cadastra uma nova categoria de produto.
        /// </summary>
        /// <param name="request">Dados da categoria a ser criada.</param>
        /// <response code="201">Categoria criada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar categoria de produto", Description = "Cadastra uma nova categoria de produto para o comércio autenticado.")]
        [ProducesResponseType(typeof(CategoriaProdutoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CategoriaProdutoRequest request)
        {
            var comercioId = GetComercioId();
            var categoria = await _categoriaProdutoService.CreateAsync(request, comercioId);
            return CreatedAtAction(nameof(GetById), new { id = categoria.CategoriaID }, categoria);
        }

        /// <summary>
        /// Atualiza os dados de uma categoria de produto existente.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Categoria atualizada com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="404">Categoria não encontrada no comércio autenticado.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar categoria de produto", Description = "Atualiza os dados de uma categoria de produto do comércio autenticado.")]
        [ProducesResponseType(typeof(CategoriaProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CategoriaProdutoRequest request)
        {
            var comercioId = GetComercioId();
            var categoria = await _categoriaProdutoService.UpdateAsync(id, request, comercioId);
            return Ok(categoria);
        }

        /// <summary>
        /// Remove uma categoria de produto.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <response code="204">Categoria removida com sucesso.</response>
        /// <response code="404">Categoria não encontrada no comércio autenticado.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Remover categoria de produto", Description = "Remove uma categoria de produto do comércio autenticado.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var comercioId = GetComercioId();
            await _categoriaProdutoService.DeleteAsync(id, comercioId);
            return NoContent();
        }
    }
}
