using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.SwitchComercio;
using ninx.Communication.Request;

namespace ninx.Api.Controllers.SwitchControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SwitchController : ControllerBase
    {   
        private readonly ISwitchComercioService _switchComercioService;
        public SwitchController(ISwitchComercioService switchComercioService)
        {
            _switchComercioService = switchComercioService;
        }

        [HttpPost("switch-comercio")]
        public async Task<IActionResult> SwitchComercio([FromBody] SwitchRequest request)
        {
            var token = await _switchComercioService.TrocarAsync(request);
            if (token == null)
            {
                return BadRequest("Usuário não encontrado ou sem acesso ao comércio.");
            }
            return Ok(new {Token = token});
        }
    }
}
