using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Fluxo de assinatura eletrônica de documentos de venda.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Fluxo de assinatura eletrônica de documentos de venda.")]
    public class AssinaturaEletronicaController : NinxControllerBase
    {
        private readonly IAssinaturaEletronicaService _assinaturaService;

        public AssinaturaEletronicaController(IAssinaturaEletronicaService assinaturaService)
        {
            _assinaturaService = assinaturaService;
        }

        /// <summary>
        /// Obtém os dados de um documento pendente de assinatura.
        /// </summary>
        /// <remarks>Endpoint público, não exige autenticação — usado pelo assinante do documento.</remarks>
        /// <param name="guid">Identificador público do documento.</param>
        /// <response code="200">Dados do documento retornados com sucesso.</response>
        /// <response code="404">Documento não encontrado.</response>
        [AllowAnonymous]
        [HttpGet("{guid}")]
        [SwaggerOperation(Summary = "Obter documento para assinatura", Description = "Retorna os dados necessários para o assinante visualizar e assinar o documento.")]
        [ProducesResponseType(typeof(AssinaturaEletronicaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<AssinaturaEletronicaResponse>> ObterDocumento(Guid guid)
        {
            var response = await _assinaturaService.ObterDadosParaAssinaturaAsync(guid);
            return Ok(response);
        }

        /// <summary>
        /// Obtém o documento já assinado.
        /// </summary>
        /// <param name="guid">Identificador público do documento.</param>
        /// <response code="200">Documento assinado retornado com sucesso.</response>
        /// <response code="404">Documento não encontrado no comércio autenticado.</response>
        [HttpGet("comercio/{guid}")]
        [SwaggerOperation(Summary = "Obter documento assinado", Description = "Retorna o documento já assinado, para o comércio autenticado.")]
        [ProducesResponseType(typeof(AssinaturaEletronicaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterDocumentoAssinado(Guid guid)
        {
            var comercioId = GetComercioId();
            var response = await _assinaturaService.ObterDocumentoAssinadoAsync(guid, comercioId);
            return Ok(response);
        }

        /// <summary>
        /// Confirma a assinatura de um documento.
        /// </summary>
        /// <remarks>Endpoint público, não exige autenticação — usado pelo assinante do documento.</remarks>
        /// <param name="guid">Identificador público do documento.</param>
        /// <param name="request">Imagem da assinatura, em base64.</param>
        /// <response code="200">Assinatura registrada com sucesso.</response>
        /// <response code="404">Documento não encontrado.</response>
        [AllowAnonymous]
        [HttpPost("confirmar/{guid}")]
        [SwaggerOperation(Summary = "Confirmar assinatura", Description = "Registra a assinatura do assinante para o documento informado, junto com IP e dispositivo de origem.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ConfirmarAssinatura(Guid guid, [FromBody] ConfirmarAssinaturaEletronicaRequest request)
        {
            var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                     ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            var dispositivo = Request.Headers["User-Agent"].ToString();

            await _assinaturaService.ConfirmarAssinaturaAsync(
                guid,
                request.ImagemBase64,
                ip ?? "IP não identificado",
                dispositivo);

            return Ok(new { mensagem = "Assinatura registrada com sucesso!" });
        }

        /// <summary>
        /// Verifica se um documento já foi assinado.
        /// </summary>
        /// <remarks>Endpoint público, não exige autenticação.</remarks>
        /// <param name="guid">Identificador público do documento.</param>
        /// <response code="200">Documento já assinado.</response>
        /// <response code="400">Documento ainda não foi assinado.</response>
        [AllowAnonymous]
        [HttpGet("assinado/{guid}")]
        [SwaggerOperation(Summary = "Validar se documento foi assinado", Description = "Verifica se o documento informado já possui uma assinatura registrada.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidaAssinado(Guid guid)
        {
            var isAssinado = await _assinaturaService.ValidaAssinado(guid);

            if (!isAssinado) return BadRequest(new { mensagem = "Este documento ainda não foi assinado." });

            return Ok(new { mesagem = "Assinatura concluída" });
        }
    }
}
