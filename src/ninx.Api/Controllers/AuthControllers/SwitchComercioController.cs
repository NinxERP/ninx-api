using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services.SwitchComercio;
using ninx.Communication.Request;

namespace ninx.Api.Controllers.AuthControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SwitchComercioController : ControllerBase
    {   
        private readonly ISwitchComercioService _switchComercioService;
        public SwitchComercioController(ISwitchComercioService switchComercioService)
        {
            _switchComercioService = switchComercioService;
        }

        [HttpPost("Switch")]
        public async Task<IActionResult> SwitchComercio([FromBody] SwitchComercioRequest request)
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
