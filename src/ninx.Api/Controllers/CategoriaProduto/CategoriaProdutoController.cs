using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Communication.Response;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriaProdutoController : NinxControllerBase
    {
        private readonly ICategoriaProdutoService _categoriaProdutoService;

        public CategoriaProdutoController(ICategoriaProdutoService categoriaProdutoService)
        {
            _categoriaProdutoService = categoriaProdutoService;
        }

        [HttpGet]
        [Route("All")]
        [ProducesResponseType(typeof(IEnumerable<CategoriaProdutoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize)
        {
            var categorias = await _categoriaProdutoService.GetAllAsync(pageNumber, pageSize);
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoriaProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var categoria = await _categoriaProdutoService.GetByIdAsync(id);
            return Ok(categoria);
        }

        [HttpPost]
        [ProducesResponseType(typeof(CategoriaProdutoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CategoriaProdutoRequest request)
        {
            var categoria = await _categoriaProdutoService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = categoria.CategoriaID }, categoria);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CategoriaProdutoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] CategoriaProdutoRequest request)
        {
            var categoria = await _categoriaProdutoService.UpdateAsync(id, request);
            return Ok(categoria);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoriaProdutoService.DeleteAsync(id);
            return NoContent();
        }
    }
}
