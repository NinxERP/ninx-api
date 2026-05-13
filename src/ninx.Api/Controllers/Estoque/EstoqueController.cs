using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;

namespace ninx.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EstoqueController : NinxControllerBase
    {
        private readonly IEstoqueService _estoqueService;

        public EstoqueController(IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }

        [HttpGet]
        [Route("All")]
        [ProducesResponseType(typeof(IEnumerable<EstoqueResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(PaginationRequest request)
        {
            var comercioId = GetComercioId();
            var estoques = await _estoqueService.GetAllByComercioIdAsync(comercioId, request);
            return Ok(estoques);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EstoqueResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var estoque = await _estoqueService.GetByIdAsync(id);
            return Ok(estoque);
        }

        [HttpPost]
        [ProducesResponseType(typeof(EstoqueResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] EstoqueRequest request)
        {
            var comercioId = GetComercioId();
            var estoque = await _estoqueService.CreateAsync(request, comercioId);
            return CreatedAtAction(nameof(GetById), new { id = estoque.EstoqueID }, estoque);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(EstoqueResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] EstoqueRequest request)
        {
            var comercioId = GetComercioId();
            var estoque = await _estoqueService.UpdateAsync(id, request, comercioId);
            return Ok(estoque);
        }

        [HttpDelete("{id}")]
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
