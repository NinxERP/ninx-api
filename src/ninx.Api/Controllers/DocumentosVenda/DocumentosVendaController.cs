using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;

namespace ninx.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentosVendaController : ControllerBase
    {
        private readonly IDocumentosVendaService _documentosVidaService;

        public DocumentosVendaController(IDocumentosVendaService documentosVidaService)
        {
            _documentosVidaService = documentosVidaService;
        }
        [AllowAnonymous]
        [HttpGet("{guid}")]
        public async Task<ActionResult<DocumentosVendaResponse>> ObterDocumento(Guid guid)
        {
            var response = await _documentosVidaService.ObterDadosParaAssinaturaAsync(guid);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("confirmar/{guid}")]
        public async Task<IActionResult> ConfirmarAssinatura(Guid guid, [FromBody] ConfirmarAssinaturaEletronicaRequest request)
        {
            var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                     ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            var dispositivo = Request.Headers["User-Agent"].ToString();

            await _documentosVidaService.ConfirmarAssinaturaAsync(
                guid,
                request.ImagemBase64,
                ip ?? "IP não identificado",
                dispositivo);

            return Ok(new { mensagem = "Assinatura registrada com sucesso!" });
        }

        [AllowAnonymous]
        [HttpGet("assinado/{guid}")]
        public async Task<IActionResult> ValidaAssinado(Guid guid)
        {
            var isAssinado = await _documentosVidaService.ValidaAssinado(guid);

            if (!isAssinado) return BadRequest(new { mensagem = "Este documento ainda não foi assinado." });

            return Ok(new { mesagem = "Assinatura concluída" });
        }
    }
}