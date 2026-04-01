using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ninx.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PingController : NinxControllerBase
    {
        [HttpGet()]
        [AllowAnonymous]
        public IActionResult Ping()
        {
            return Ok();
        }

        [HttpGet("Autenticado")]
        public IActionResult PingAutenticado()
        {
            return Ok();
        }

        [HttpPost()]
        [AllowAnonymous]
        public IActionResult PingPost()
        {
            return Ok();
        }

    }
}
