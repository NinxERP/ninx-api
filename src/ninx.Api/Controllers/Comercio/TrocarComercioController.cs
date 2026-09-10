using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Troca do comércio ativo na sessão do usuário autenticado.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    [SwaggerTag("Troca do comércio ativo na sessão do usuário autenticado, gerando um novo token.")]
    public class TrocarComercioController : NinxControllerBase
    {
        private readonly ITrocarComercioService _trocarComercioService;
        public TrocarComercioController(ITrocarComercioService trocarComercioService)
        {
            _trocarComercioService = trocarComercioService;
        }

        /// <summary>
        /// Troca o comércio ativo do usuário autenticado.
        /// </summary>
        /// <param name="comercioID">Identificador do comércio para o qual o usuário deseja trocar.</param>
        /// <response code="200">Comércio trocado com sucesso; um novo token é retornado.</response>
        [HttpPost("{comercioID}")]
        [SwaggerOperation(Summary = "Trocar comércio", Description = "Troca o comércio ativo do usuário autenticado e retorna um novo token JWT já apontando para o novo comércio.")]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(IEnumerable<ClienteResponse>), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> TrocarComercio([FromRoute] int comercioID)
        {
            var usuarioId = GetUsuarioId();
            var token = await _trocarComercioService.TrocarAsync(comercioID, usuarioId);
            return Ok(new {Token = token});
        }
    }
}
