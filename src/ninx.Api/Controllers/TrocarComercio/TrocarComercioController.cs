using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.Login;
using ninx.Communication.Request.Login;

namespace ninx.Api.Controllers
{
    [ApiController]
    [Route("api/TrocarComercio/[controller]")]
    public class TrocarComercioController : ControllerBase
    {   
        private readonly ITrocarComercioService _trocarComercioService;
        public TrocarComercioController(ITrocarComercioService trocarComercioService)
        {
            _trocarComercioService = trocarComercioService;
        }

        [HttpPost]
        public async Task<IActionResult> TrocarComercio([FromBody] TrocarComercioRequest request)
        {
            var token = await _trocarComercioService.TrocarAsync(request);
            if (token == null)
            {
                return BadRequest("Usuário não encontrado ou sem acesso ao comércio.");
            }
            return Ok(new {Token = token});
        }
    }
}
