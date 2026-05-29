using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;

namespace ninx.Api.Controllers  
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrocarComercioController : NinxControllerBase
    {   
        private readonly ITrocarComercioService _trocarComercioService;
        public TrocarComercioController(ITrocarComercioService trocarComercioService)
        {
            _trocarComercioService = trocarComercioService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> TrocarComercio([FromBody] int comercioID)
        {
            var usuarioId = GetUsuarioId();
            var token = await _trocarComercioService.TrocarAsync(comercioID, usuarioId);
            return Ok(new {Token = token});
        }
    }
}
