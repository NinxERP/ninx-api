using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication; 

namespace ninx.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssinaturaPlanoController : ControllerBase
    {
        private readonly IAssinaturaPlanoService _assinaturaService;

        public AssinaturaPlanoController(IAssinaturaPlanoService assinaturaService)
        {
            _assinaturaService = assinaturaService;
        }

        [HttpGet]
        [Route("All")]
        [ProducesResponseType(typeof(IEnumerable<AssinaturaPlanoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(PaginationRequest request)
        {
            var result = await _assinaturaService.GetAll(request);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AssinaturaPlanoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _assinaturaService.GetByIdAsync(id);
            return Ok(result);
        }
    }
}