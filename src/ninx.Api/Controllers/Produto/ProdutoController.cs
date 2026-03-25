using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Interfaces.Services;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Produto/[controller]")]
    public class ProdutoController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutoController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpGet("comercio/{comercioId}")]
        [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllByComercio(int comercioId)
        {
            var produtos = await _produtoService.GetByComercioIdAsync(comercioId);
            if (produtos is null)
            {
                return NotFound();
            }
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var produto = await _produtoService.GetByIdAsync(id);
            if (produto is null)
            {
                return NotFound();
            }
            return Ok(produto);
        }

        [HttpGet("comercio/{comercioId}/codigo-barras/{codigoBarras}")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCodigoBarras(int comercioId, string codigoBarras)
        {
            var produto = await _produtoService.GetByCodigoBarrasAsync(comercioId, codigoBarras);
            if (produto is null)
            {
                return NotFound();
            }
            return Ok(produto);
        }

        [HttpGet("comercio/{comercioId}/buscar")]
        [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNome(int comercioId, [FromQuery] string nome)
        {
            var produtos = await _produtoService.GetByNomeAsync(comercioId, nome);
            if (produtos is null)
            {
                return NotFound();
            }
            return Ok(produtos);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarProdutoRequest request)
        {
            var produto = await _produtoService.CriarAsync(request);
            return CreatedAtAction(string.Empty, produto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarProdutoRequest request)
        {
            var produto = await _produtoService.AtualizarAsync(id, request);
            return Ok(produto);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            await _produtoService.DesativarAsync(id);
            return NoContent();
        }
    }
}