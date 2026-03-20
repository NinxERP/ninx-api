using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.Comercio;

namespace ninx.Api.Controllers.ComercioControllers
{
    [Route("api/Comercio/[controller]")]
    [ApiController]
    public class ComercioController : ControllerBase
    {
        public readonly IComercioService _comercioService;
        public ComercioController(IComercioService comercioService)
        {
            _comercioService = comercioService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _comercioService.GetAll();
            if (!result.Any())
            {
                return NotFound("Nenhum comércio encontrado.");
            }
            return Ok(result);
        }

        [HttpGet("GetByUsuarioId")]
        public async Task<IActionResult> GetByUsuarioId(int usuarioId)
        {
            var result = await _comercioService.GetByUsuarioId(usuarioId);
            if (!result.Any())
            {
                return NotFound("Nenhum comércio encontrado.");
            }
            return Ok(result);
        }
    }
}
