using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ninx.Api.Controllers
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
