using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;

namespace ninx.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssinaturaEletronicaController : ControllerBase
    {
        private readonly IAssinaturaEletronicaService _assinaturaService;

        public AssinaturaEletronicaController(IAssinaturaEletronicaService assinaturaService)
        {
            _assinaturaService = assinaturaService;
        }
        [AllowAnonymous]
        [HttpGet("{guid}")]
        public async Task<ActionResult<AssinaturaEletronicaResponse>> ObterDocumento(Guid guid)
        {
            var response = await _assinaturaService.ObterDadosParaAssinaturaAsync(guid);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("confirmar/{guid}")]
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

        [AllowAnonymous]
        [HttpGet("assinado/{guid}")]
        public async Task<IActionResult> ValidaAssinado(Guid guid)
        {
            var isAssinado = await _assinaturaService.ValidaAssinado(guid);

            if (!isAssinado) return BadRequest(new { mensagem = "Este documento ainda não foi assinado." });

            return Ok(new { mesagem = "Assinatura concluída" });
        }
    }
}