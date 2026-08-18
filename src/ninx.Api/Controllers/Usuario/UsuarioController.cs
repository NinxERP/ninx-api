using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ninx.Application.Services;
using ninx.Communication;
using Swashbuckle.AspNetCore.Annotations;

namespace ninx.Api.Controllers
{
    /// <summary>
    /// Cadastro e consulta de usuários.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Cadastro e consulta de usuários.")]
    public class UsuarioController : NinxControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        /// <summary>
        /// Busca um usuário pelo identificador, sem restringir pelo comércio.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="200">Usuário encontrado.</response>
        /// <response code="401">Apenas administradores globais podem utilizar esse endpoint.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpGet("NoComercioId/{id}")]
        [SwaggerOperation(Summary = "Buscar usuário por id (sem filtro de comércio)", Description = "Retorna um usuário pelo identificador, sem restringir a busca ao comércio ativo na sessão. Restrito a administradores globais (Usuario.Permissao == Administrador).")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var usuarioIdLogado = GetUsuarioId();
            var usuario = await _usuarioService.GetById(id, usuarioIdLogado);
            return Ok(usuario);
        }

        /// <summary>
        /// Lista todos os usuários de todos os comércios. Restrito a administradores globais.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Usuários retornados com sucesso.</response>
        /// <response code="401">Apenas administradores globais podem utilizar esse endpoint.</response>
        /// <response code="404">Nenhum usuário encontrado.</response>
        [HttpGet("NoComercioId/All")]
        [SwaggerOperation(Summary = "Listar usuários (sem filtro de comércio)", Description = "Retorna os usuários de todos os comércios, sem restringir ao comércio ativo na sessão. Restrito a administradores globais (Usuario.Permissao == Administrador) — não use esse endpoint para a tela padrão de gestão de equipe, use GET /api/Usuario/All.")]
        [ProducesResponseType(typeof(PaginatedResponse<UsuarioResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll([FromQuery] PaginationRequest request)
        {
            var usuarioIdLogado = GetUsuarioId();
            var usuario = await _usuarioService.GetAll(usuarioIdLogado, request);
            return Ok(usuario);
        }

        /// <summary>
        /// Lista os usuários do comércio autenticado.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Usuários retornados com sucesso.</response>
        /// <response code="403">Funcionários não podem consultar usuários.</response>
        /// <response code="404">Nenhum usuário encontrado.</response>
        [HttpGet("All")]
        [SwaggerOperation(Summary = "Listar usuários do comércio", Description = "Retorna os usuários vinculados ao comércio autenticado. Administradores veem todos; Donos (gerentes) veem apenas usuários com permissão de funcionário; funcionários não têm acesso.")]
        [ProducesResponseType(typeof(PaginatedResponse<UsuarioResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllByComercioId([FromQuery] PaginationRequest request)
        {
            var comercioId = GetComercioId();
            var permissao = GetPermissao();
            var usuario = await _usuarioService.GetAllByComercioId(comercioId, permissao, request);
            return Ok(usuario);
        }

        /// <summary>
        /// Busca um usuário do comércio autenticado pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="200">Usuário encontrado.</response>
        /// <response code="403">Sem permissão para consultar este usuário.</response>
        /// <response code="404">Usuário não encontrado no comércio autenticado.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Buscar usuário por id", Description = "Retorna um usuário do comércio autenticado pelo identificador. Donos (gerentes) só podem consultar usuários com permissão de funcionário.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAndComercioIdAsync(int id)
        {
            var comercioId = GetComercioId();
            var permissao = GetPermissao();
            var usuario = await _usuarioService.GetByIdAndComercioIdAsync(id, comercioId, permissao);
            return Ok(usuario);
        }


        /// <summary>
        /// Cadastra um novo usuário no comércio autenticado.
        /// </summary>
        /// <param name="request">Dados do usuário a ser criado.</param>
        /// <response code="201">Usuário criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        /// <response code="403">Funcionários não podem cadastrar usuários.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar usuário", Description = "Cadastra um novo usuário vinculado ao comércio autenticado. Administradores podem criar usuários com qualquer permissão; Donos (gerentes) só podem criar usuários com permissão de funcionário (a permissão informada é forçada para Funcionario); funcionários não podem criar usuários.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Criar([FromBody] CriarUsuarioRequest request)
        {
            var usuarioId = GetUsuarioId();
            var comercioId = GetComercioId();
            var permissao = GetPermissao();

            var usuario = await _usuarioService.CriarAsync(request, usuarioId, permissao, comercioId);
            return CreatedAtAction(nameof(GetById), new { id = usuario.UsuarioID }, usuario);
        }

        /// <summary>
        /// Atualiza os dados de um usuário existente.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Usuário atualizado com sucesso.</response>
        /// <response code="403">Sem permissão para atualizar este usuário.</response>
        /// <response code="404">Usuário não encontrado no comércio autenticado.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar usuário", Description = "Atualiza os dados de um usuário do comércio autenticado. Donos (gerentes) só podem atualizar usuários com permissão de funcionário; funcionários não podem atualizar usuários.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioRequest request)
        {
            var comercioId = GetComercioId();
            var permissao = GetPermissao();
            var usuario = await _usuarioService.AtualizarAsync(id, request, comercioId, permissao);
            return Ok(usuario);
        }

        /// <summary>
        /// Desativa um usuário.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="204">Usuário desativado com sucesso.</response>
        /// <response code="403">Sem permissão para desativar este usuário.</response>
        /// <response code="404">Usuário não encontrado no comércio autenticado.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Desativar usuário", Description = "Desativa (soft delete) um usuário do comércio autenticado. Donos (gerentes) só podem desativar usuários com permissão de funcionário; funcionários não podem desativar usuários.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            var comercioId = GetComercioId();
            var permissao = GetPermissao();
            await _usuarioService.DesativarAsync(id, comercioId, permissao);
            return NoContent();
        }
    }
}
