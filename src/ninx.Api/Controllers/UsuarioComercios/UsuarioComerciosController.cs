using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Vínculo entre usuário e comércio (permissões de acesso).
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Vínculo entre usuário e comércio, incluindo permissão de acesso.")]
    public class UsuarioComercioController : NinxControllerBase
    {
        private readonly IUsuarioComercioService _usuarioComercioService;

        public UsuarioComercioController(IUsuarioComercioService usuarioComercioService)
        {
            _usuarioComercioService = usuarioComercioService;
        }

        /// <summary>
        /// Vincula um usuário existente a um comércio.
        /// </summary>
        /// <param name="request">Dados do vínculo a ser criado, incluindo permissão.</param>
        /// <response code="201">Vínculo criado com sucesso.</response>
        /// <response code="400">Dados inválidos ou usuário já vinculado a esse comércio.</response>
        /// <response code="403">Usuário autenticado não tem permissão para vincular usuários a esse comércio.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Vincular usuário a comércio", Description = "Vincula um usuário já cadastrado na plataforma a um comércio, com a permissão informada. Restrito a Administrador/Dono do comércio; Donos só podem vincular com permissão de funcionário (a permissão informada é forçada para Funcionario).")]
        [ProducesResponseType(typeof(UsuarioComercioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Criar([FromBody] CriarUsuarioComercioRequest request)
        {
            var usuarioLogadoId = GetUsuarioId();
            var result = await _usuarioComercioService.CriarAsync(request, usuarioLogadoId);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Atualiza o vínculo entre um usuário e um comércio.
        /// </summary>
        /// <param name="request">Dados do vínculo a serem atualizados, incluindo permissão.</param>
        /// <response code="200">Vínculo atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="403">Usuário autenticado não tem permissão para essa alteração.</response>
        /// <response code="404">Vínculo entre usuário e comércio não encontrado.</response>
        [HttpPut]
        [SwaggerOperation(Summary = "Atualizar vínculo usuário-comércio", Description = "Atualiza o vínculo (incluindo permissão) entre um usuário e um comércio.")]
        [ProducesResponseType(typeof(UsuarioComercioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Atualizar([FromBody] AtualizarUsuarioComercioRequest request)
        {
            var usuarioLogadoId = GetUsuarioId();
            var result = await _usuarioComercioService.AtualizarAsync(request, usuarioLogadoId);
            return Ok(result);
        }

        /// <summary>
        /// Desativa o vínculo entre um usuário e um comércio.
        /// </summary>
        /// <param name="usuarioId">Identificador do usuário.</param>
        /// <param name="comercioId">Identificador do comércio.</param>
        /// <response code="204">Vínculo desativado com sucesso.</response>
        /// <response code="404">Vínculo entre usuário e comércio não encontrado.</response>
        [HttpDelete]
        [SwaggerOperation(Summary = "Desativar vínculo usuário-comércio", Description = "Remove o acesso de um usuário a um comércio, desativando o vínculo entre eles.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Desativar([FromQuery] int usuarioId, [FromQuery] int comercioId)
        {
            var usuarioLogadoId = GetUsuarioId();
            await _usuarioComercioService.DesativarAsync(usuarioId, comercioId, usuarioLogadoId);
            return NoContent();
        }
    }
}
