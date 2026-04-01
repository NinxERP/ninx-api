using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Interfaces.Services;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutoController : NinxControllerBase
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
            var produtos = await _produtoService.GetProdutosByComercioIdAsync(comercioId);
            return Ok(produtos);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var produto = await _produtoService.GetByIdAsync(id);
            return Ok(produto);
        }

        [HttpGet("comercio/{comercioId}/codigo-barras/{codigoBarras}")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCodigoBarras(int comercioId, string codigoBarras)
        {
            var produto = await _produtoService.GetByCodigoBarrasAsync(comercioId, codigoBarras);
            return Ok(produto);
        }

        [HttpGet("comercio/{comercioId}/buscar")]
        [ProducesResponseType(typeof(IEnumerable<ProdutoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByNome(int comercioId, [FromQuery] string nome)
        {
            var produtos = await _produtoService.GetByNomeAsync(comercioId, nome);
            return Ok(produtos);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Criar([FromBody] CriarProdutoRequest request)
        {
            var comercioID = GetComercioId();
            var produto = await _produtoService.CriarAsync(request, comercioID);
            return CreatedAtAction(string.Empty, produto);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarProdutoRequest request)
        {
            var comercioId = GetComercioId();
            var produto = await _produtoService.AtualizarAsync(id, comercioId, request);
            return Ok(produto);
        }

        [HttpDelete("{id}")]
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