using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ninx.Api.Controllers.Ping
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : ControllerBase
    {
        [HttpGet("Ping")]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpGet("PingAutenticado")]
        public IActionResult PingAutenticado()
        {
            return Ok();
        }

        [HttpPost("Ping")]
        [AllowAnonymous]
        public IActionResult PingPost()
        {
            return Ok();
        }

    }
}
