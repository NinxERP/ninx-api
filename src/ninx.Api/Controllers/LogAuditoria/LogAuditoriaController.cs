using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Trilha de auditoria de ações sensíveis da plataforma.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Trilha de auditoria de ações sensíveis da plataforma.")]
    public class LogAuditoriaController : NinxControllerBase
    {
        private readonly ILogAuditoriaService _logAuditoriaService;

        public LogAuditoriaController(ILogAuditoriaService logAuditoriaService)
        {
            _logAuditoriaService = logAuditoriaService;
        }

        /// <summary>
        /// Lista os registros de auditoria da plataforma.
        /// </summary>
        /// <param name="comercioId">Filtra pelo comércio afetado pela ação.</param>
        /// <param name="usuarioId">Filtra pelo usuário que executou a ação.</param>
        /// <param name="request">Parâmetros de paginação.</param>
        /// <response code="200">Registros retornados com sucesso.</response>
        /// <response code="401">Apenas administradores globais podem utilizar esse endpoint.</response>
        [HttpGet("All")]
        [SwaggerOperation(Summary = "Listar registros de auditoria", Description = "Retorna os registros de auditoria de ações sensíveis (criação/desativação de comércios, alteração de identidade e de vínculo de usuários), opcionalmente filtrados por comércio ou usuário. Restrito a administradores globais (Usuario.Admin == true).")]
        [ProducesResponseType(typeof(PaginatedResponse<LogAuditoriaResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] int? comercioId, [FromQuery] int? usuarioId, [FromQuery] PaginationRequest request)
        {
            var usuarioIdLogado = GetUsuarioId();
            var result = await _logAuditoriaService.GetAllAsync(usuarioIdLogado, comercioId, usuarioId, request);
            return Ok(result);
        }
    }
}
