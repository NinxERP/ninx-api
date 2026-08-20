using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Cargos (hierarquia por peso) disponíveis para os vínculos usuário-comércio.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Cargos (hierarquia por peso) disponíveis para os vínculos usuário-comércio. Cargos base (ComercioID nulo) são compartilhados por todos os comércios e restritos a administradores de plataforma para criação/edição/desativação; cargos customizados pertencem a um comércio.")]
    public class CargoController : NinxControllerBase
    {
        private readonly ICargoService _cargoService;

        public CargoController(ICargoService cargoService)
        {
            _cargoService = cargoService;
        }

        /// <summary>
        /// Lista cargos: os disponíveis para um comércio (base + customizados), ou os cargos base, caso nenhum comércio seja informado.
        /// </summary>
        /// <param name="comercioId">Identificador do comércio. Se omitido, retorna os cargos base (inclusive inativos).</param>
        /// <response code="200">Cargos retornados com sucesso.</response>
        [HttpGet]
        [SwaggerOperation(Summary = "Listar cargos", Description = "Com comercioId informado, retorna os cargos ativos disponíveis para esse comércio (base + customizados). Sem comercioId, retorna todos os cargos base (globais, compartilhados por todos os comércios), inclusive inativos.")]
        [ProducesResponseType(typeof(IEnumerable<CargoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] int? comercioId)
        {
            var result = await _cargoService.GetAsync(comercioId);
            return Ok(result);
        }

        /// <summary>
        /// Cria um cargo customizado de um comércio, ou um cargo base.
        /// </summary>
        /// <param name="request">Dados do cargo a ser criado. ComercioID nulo cria um cargo base.</param>
        /// <response code="201">Cargo criado com sucesso.</response>
        /// <response code="400">Dados inválidos ou nome já em uso.</response>
        /// <response code="401">Cargo base informado (ComercioID nulo) sem ser administrador de plataforma.</response>
        /// <response code="403">Sem permissão para criar cargos neste comércio, ou peso informado maior ou igual ao do chamador.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar cargo", Description = "Com ComercioID informado, cria um cargo exclusivo desse comércio — restrito a quem tem cargo de peso Dono ou superior no comércio (ou administrador de plataforma), com peso estritamente menor que o do chamador. Com ComercioID nulo, cria um cargo base — restrito a administradores de plataforma (Usuario.Admin == true).")]
        [ProducesResponseType(typeof(CargoResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] CriarCargoRequest request)
        {
            var usuarioLogadoId = GetUsuarioId();
            var result = await _cargoService.CriarAsync(request, usuarioLogadoId);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Atualiza um cargo customizado de um comércio, ou um cargo base.
        /// </summary>
        /// <param name="id">Identificador do cargo.</param>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Cargo atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos ou nome já em uso.</response>
        /// <response code="401">Cargo base sem ser administrador de plataforma.</response>
        /// <response code="403">Sem permissão para editar cargos deste comércio, peso informado maior ou igual ao do chamador, ou cargo reservado pelo sistema.</response>
        /// <response code="404">Cargo não encontrado.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar cargo", Description = "Atualiza nome e peso de um cargo. Cargos customizados seguem a mesma regra de criação (peso Dono ou superior no comércio / peso menor que o do chamador); cargos base são restritos a administradores de plataforma e não podem ser reservados pelo sistema (ex.: o cargo 'Admin').")]
        [ProducesResponseType(typeof(CargoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarCargoRequest request)
        {
            var usuarioLogadoId = GetUsuarioId();
            var result = await _cargoService.AtualizarAsync(id, request, usuarioLogadoId);
            return Ok(result);
        }

        /// <summary>
        /// Desativa (soft delete) um cargo customizado de um comércio, ou um cargo base.
        /// </summary>
        /// <param name="id">Identificador do cargo.</param>
        /// <response code="204">Cargo desativado com sucesso.</response>
        /// <response code="401">Cargo base sem ser administrador de plataforma.</response>
        /// <response code="403">Sem permissão para desativar cargos deste comércio, ou cargo reservado pelo sistema.</response>
        /// <response code="404">Cargo não encontrado.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Desativar cargo", Description = "Desativa um cargo. Vínculos existentes que já usam esse cargo continuam funcionando; o cargo apenas deixa de ser oferecido para novos vínculos.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            var usuarioLogadoId = GetUsuarioId();
            await _cargoService.DesativarAsync(id, usuarioLogadoId);
            return NoContent();
        }
    }
}
