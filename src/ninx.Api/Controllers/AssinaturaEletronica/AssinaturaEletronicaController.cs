using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;

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
        [HttpGet("{guid}")]
        public async Task<ActionResult<AssinaturaEletronicaResponse>> ObterDadosParaAssinatura(Guid guid)
        {
            var response = await _assinaturaService.ObterDadosParaAssinaturaAsync(guid);
            return Ok(response);
        }

        [HttpPost("confirmar/{guid}")]
        public async Task<IActionResult> ConfirmarAssinatura(Guid guid, [FromBody] ConfirmarAssinaturaEletronicaRequest request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var dispositivo = Request.Headers["User-Agent"].ToString();

            await _assinaturaService.ConfirmarAssinaturaAsync(
                guid,
                request.ImagemBase64,
                ip ?? "IP não identificado",
                dispositivo);

            return Ok(new { mensagem = "Assinatura registrada com sucesso!" });
        }
    }
}