using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Cadastro e consulta de produtos do comércio.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Cadastro e consulta de produtos do comércio.")]
    public class ProdutoController : NinxControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutoController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        /// <summary>
        /// Lista os produtos do comércio autenticado, paginados.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Página de produtos retornada com sucesso.</response>
        [HttpGet("GetAllPaginated")]
        [SwaggerOperation(Summary = "Listar produtos (paginado)", Description = "Retorna os produtos do comércio autenticado, com suporte a paginação e busca.")]
        [ProducesResponseType(typeof(PaginatedResponse<ProdutoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPaginatedByComercio([FromQuery] PaginationRequest request)
        {
            var comercioId = GetComercioId();
            var produtos = await _produtoService.GetProdutosEstoqueByComercioIdAsync(comercioId, request);
            return Ok(produtos);
        }

        /// <summary>
        /// Busca um produto pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do produto.</param>
        /// <response code="200">Produto encontrado.</response>
        /// <response code="404">Produto não encontrado no comércio autenticado.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Buscar produto por id", Description = "Retorna um produto do comércio autenticado pelo identificador.")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var comercioId = GetComercioId();
            var produto = await _produtoService.GetByIdAsync(id, comercioId);
            return Ok(produto);
        }

        /// <summary>
        /// Busca um produto ativo pelo código de barras.
        /// </summary>
        /// <param name="codigoBarras">Código de barras do produto.</param>
        /// <response code="200">Produto encontrado.</response>
        /// <response code="404">Nenhum produto ativo com esse código de barras.</response>
        [HttpGet("codigo-barras/{codigoBarras}")]
        [SwaggerOperation(Summary = "Buscar produto por código de barras", Description = "Retorna um produto ativo do comércio autenticado a partir do código de barras.")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCodigoBarras(string codigoBarras)
        {
            var comercioId = GetComercioId();
            var produto = await _produtoService.GetAtivosByCodigoBarrasAsync(comercioId, codigoBarras);
            return Ok(produto);
        }

        /// <summary>
        /// Busca produtos pelo nome.
        /// </summary>
        /// <param name="nomeProduto">Termo de busca pelo nome do produto.</param>
        /// <response code="200">Produtos encontrados.</response>
        /// <response code="404">Nenhum produto encontrado com esse nome.</response>
        [HttpGet("produto/{nomeProduto}")]
        [SwaggerOperation(Summary = "Buscar produtos por nome", Description = "Retorna os produtos do comércio autenticado cujo nome corresponde ao termo informado.")]
        [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNome(string nomeProduto)
        {
            var comercioId = GetComercioId();
            var produtos = await _produtoService.GetByNomeAsync(comercioId, nomeProduto);
            return Ok(produtos);
        }

        /// <summary>
        /// Cadastra um novo produto.
        /// </summary>
        /// <param name="request">Dados do produto a ser criado.</param>
        /// <response code="201">Produto criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar produto", Description = "Cadastra um novo produto para o comércio autenticado.")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarProdutoRequest request)
        {
            var comercioID = GetComercioId();
            var produto = await _produtoService.CriarAsync(request, comercioID);
            return CreatedAtAction(nameof(GetById), new { id = produto.ProdutoID }, produto);
        }

        /// <summary>
        /// Atualiza os dados de um produto existente.
        /// </summary>
        /// <param name="id">Identificador do produto.</param>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Produto atualizado com sucesso.</response>
        /// <response code="404">Produto não encontrado no comércio autenticado.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar produto", Description = "Atualiza os dados de um produto do comércio autenticado.")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarProdutoRequest request)
        {
            var comercioId = GetComercioId();
            var produto = await _produtoService.AtualizarAsync(id, comercioId, request);
            return Ok(produto);
        }

        /// <summary>
        /// Desativa um produto.
        /// </summary>
        /// <param name="id">Identificador do produto.</param>
        /// <response code="204">Produto desativado com sucesso.</response>
        /// <response code="404">Produto não encontrado no comércio autenticado.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Desativar produto", Description = "Desativa (soft delete) um produto do comércio autenticado.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            var comercioId = GetComercioId();
            await _produtoService.DesativarAsync(id, comercioId);
            return NoContent();
        }
    }
}
