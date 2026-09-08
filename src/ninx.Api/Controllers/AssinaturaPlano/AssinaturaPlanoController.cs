using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Planos de assinatura disponíveis no sistema.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [SwaggerTag("Planos de assinatura disponíveis no sistema.")]
    public class AssinaturaPlanoController : NinxControllerBase
    {
        private readonly IAssinaturaPlanoService _assinaturaService;

        public AssinaturaPlanoController(IAssinaturaPlanoService assinaturaService)
        {
            _assinaturaService = assinaturaService;
        }

        /// <summary>
        /// Lista os planos de assinatura.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Planos retornados com sucesso.</response>
        [HttpGet]
        [Route("All")]
        [SwaggerOperation(Summary = "Listar planos de assinatura", Description = "Retorna os planos de assinatura cadastrados no sistema.")]
        [ProducesResponseType(typeof(IEnumerable<AssinaturaPlanoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
        {
            var result = await _assinaturaService.GetAll(request);
            return Ok(result);
        }

        /// <summary>
        /// Retorna o status da assinatura do comércio autenticado.
        /// </summary>
        /// <response code="200">Assinatura encontrada.</response>
        /// <response code="403">Funcionários não podem consultar a assinatura do comércio.</response>
        /// <response code="404">Nenhuma assinatura encontrada para o comércio.</response>
        [HttpGet("comercio")]
        [SwaggerOperation(Summary = "Assinatura do comércio autenticado", Description = "Retorna o plano, status e vigência da assinatura do comércio autenticado.")]
        [ProducesResponseType(typeof(AssinaturaPlanoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByComercioAutenticado()
        {
            var comercioId = GetComercioId();
            var pesoLogado = GetCargoPeso();
            var result = await _assinaturaService.GetByComercioIdAsync(comercioId, pesoLogado);
            return Ok(result);
        }

        /// <summary>
        /// Busca um plano de assinatura pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do plano.</param>
        /// <response code="200">Plano encontrado.</response>
        /// <response code="404">Plano não encontrado.</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Buscar plano de assinatura por id", Description = "Retorna um plano de assinatura pelo identificador.")]
        [ProducesResponseType(typeof(AssinaturaPlanoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _assinaturaService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Solicita o cancelamento da assinatura do comércio autenticado.
        /// </summary>
        /// <response code="204">Cancelamento agendado com sucesso — acesso mantido até o fim da vigência.</response>
        /// <response code="400">Assinatura já cancelada/vencida ou já possui cancelamento agendado.</response>
        /// <response code="403">Funcionários não podem cancelar a assinatura do comércio.</response>
        /// <response code="404">Nenhuma assinatura encontrada para o comércio.</response>
        [HttpPost("cancelar")]
        [SwaggerOperation(Summary = "Cancelar assinatura do comércio autenticado", Description = "Agenda o cancelamento da assinatura; o acesso permanece ativo até o fim da vigência já paga (DataFim).")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Cancelar()
        {
            var comercioId = GetComercioId();
            var pesoLogado = GetCargoPeso();
            await _assinaturaService.CancelarAsync(comercioId, pesoLogado);
            return NoContent();
        }
    }
}
