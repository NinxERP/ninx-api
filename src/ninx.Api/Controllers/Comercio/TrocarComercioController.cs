using Microsoft.AspNetCore.Mvc;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using System.Security.Claims;

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
        public async Task<IActionResult> TrocarComercio([FromBody] int comercioID)
        {
            var usuarioId = GetUsuarioId();
            var token = await _trocarComercioService.TrocarAsync(comercioID, usuarioId);
            return Ok(new {Token = token});
        }
    }
}
