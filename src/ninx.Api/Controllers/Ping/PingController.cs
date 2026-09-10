using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Endpoints de diagnóstico usados para verificar se a API está no ar.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [SwaggerTag("Endpoints de diagnóstico, sem regra de negócio. Usados aqui como referência do padrão de documentação Swagger.")]
    public class PingController : NinxControllerBase
    {
        /// <summary>
        /// Verifica se a API está respondendo.
        /// </summary>
        /// <remarks>Endpoint público, não exige autenticação.</remarks>
        /// <response code="200">API respondendo normalmente.</response>
        [HttpGet()]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Verifica disponibilidade da API", Description = "Endpoint público de health-check, não exige token.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Ping()
        {
            return Ok();
        }

        /// <summary>
        /// Verifica se a API está respondendo em uma rota destinada a chamadas autenticadas.
        /// </summary>
        /// <response code="200">API respondendo normalmente.</response>
        /// <response code="401">Token ausente ou inválido.</response>
        [HttpGet("Autenticado")]
        [Authorize]
        [SwaggerOperation(Summary = "Verifica disponibilidade da API (rota autenticada)", Description = "Mesma finalidade do endpoint público, mas exige um token JWT válido — serve como teste de autenticação.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult PingAutenticado()
        {
            return Ok();
        }

        /// <summary>
        /// Verifica se a API aceita requisições POST.
        /// </summary>
        /// <remarks>Endpoint público, não exige autenticação.</remarks>
        /// <response code="200">API respondendo normalmente.</response>
        [HttpPost()]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Verifica disponibilidade da API via POST", Description = "Endpoint público de health-check para o verbo POST, não exige token.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult PingPost()
        {
            return Ok();
        }

    }
}
