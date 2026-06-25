using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication; 

namespace ninx.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssinaturaPlanoController : ControllerBase
    {
        private readonly IAssinaturaPlanoService _documentosVidaService;

        public AssinaturaPlanoController(IAssinaturaPlanoService assinaturaService)
        {
            _documentosVidaService = assinaturaService;
        }

        [HttpGet]
        [Route("All")]
        [ProducesResponseType(typeof(IEnumerable<AssinaturaPlanoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(PaginationRequest request)
        {
            var result = await _documentosVidaService.GetAll(request);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(AssinaturaPlanoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _documentosVidaService.GetByIdAsync(id);
            return Ok(result);
        }
    }
}