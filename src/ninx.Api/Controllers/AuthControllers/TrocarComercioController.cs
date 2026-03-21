using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.Login;
using ninx.Communication.Request.Login;

namespace ninx.Api.Controllers.AuthControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrocarComercioController : ControllerBase
    {   
        private readonly ITrocarComercioService _trocarComercioService;
        public TrocarComercioController(ITrocarComercioService trocarComercioService)
        {
            _trocarComercioService = trocarComercioService;
        }

        [HttpPost("Switch")]
        public async Task<IActionResult> SwitchComercio([FromBody] SwitchComercioRequest request)
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
