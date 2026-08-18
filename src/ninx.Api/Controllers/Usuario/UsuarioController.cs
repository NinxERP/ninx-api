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
        [SwaggerOperation(Summary = "Buscar usuário por id (sem filtro de comércio)", Description = "Retorna um usuário pelo identificador, sem restringir a busca ao comércio ativo na sessão. Restrito a administradores globais (Usuario.Admin == true).")]
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
        [SwaggerOperation(Summary = "Listar usuários (sem filtro de comércio)", Description = "Retorna os usuários de todos os comércios, sem restringir ao comércio ativo na sessão. Restrito a administradores globais (Usuario.Admin == true) — não use esse endpoint para a tela padrão de gestão de equipe, use GET /api/Usuario/All.")]
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
        /// Busca um usuário cadastrado na plataforma pelo e-mail exato, para vinculá-lo a um comércio.
        /// </summary>
        /// <param name="email">E-mail exato do usuário buscado.</param>
        /// <response code="200">Usuário encontrado.</response>
        /// <response code="403">Funcionários não podem buscar usuários.</response>
        /// <response code="404">Nenhum usuário encontrado com esse e-mail.</response>
        [HttpGet("BuscarPorEmail")]
        [SwaggerOperation(Summary = "Buscar usuário por e-mail", Description = "Busca um usuário já cadastrado na plataforma pelo e-mail exato, independente do comércio a que ele pertence. Usado para localizar o UsuarioID antes de vinculá-lo a um comércio via POST /api/UsuarioComercio. Restrito a Administradores/Donos do comércio; funcionários não têm acesso.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BuscarPorEmail([FromQuery] string email)
        {
            var permissao = GetPermissao();
            var usuario = await _usuarioService.BuscarPorEmailAsync(email, permissao);
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
        /// Atualiza os dados cadastrais de um usuário do comércio autenticado.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <param name="request">Dados a serem atualizados.</param>
        /// <response code="200">Usuário atualizado com sucesso.</response>
        /// <response code="400">E-mail já cadastrado para outro usuário.</response>
        /// <response code="403">Sem permissão para atualizar este usuário.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar usuário", Description = "Atualiza nome e e-mail de um usuário vinculado ao comércio autenticado. Administradores podem atualizar qualquer usuário do comércio; Donos (gerentes) só podem atualizar usuários com permissão de funcionário; funcionários não podem atualizar usuários. Para gerenciar a permissão/acesso de um usuário em um comércio use o PUT /api/UsuarioComercio.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioRequest request)
        {
            var comercioId = GetComercioId();
            var usuarioIdLogado = GetUsuarioId();
            var permissao = GetPermissao();
            var usuario = await _usuarioService.AtualizarAsync(id, request, comercioId, usuarioIdLogado, permissao);
            return Ok(usuario);
        }

        /// <summary>
        /// Desativa o acesso de um usuário ao comércio autenticado.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="204">Usuário desativado com sucesso.</response>
        /// <response code="403">Sem permissão para desativar este usuário.</response>
        /// <response code="404">Usuário não encontrado no comércio autenticado.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Desativar usuário", Description = "Desativa o vínculo do usuário com o comércio autenticado (revoga o acesso só a esse comércio). Se esse era o único vínculo ativo do usuário, a conta também é desativada globalmente. Donos (gerentes) só podem desativar usuários com permissão de funcionário; funcionários não podem desativar usuários.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            var comercioId = GetComercioId();
            var usuarioIdLogado = GetUsuarioId();
            await _usuarioService.DesativarAsync(id, comercioId, usuarioIdLogado);
            return NoContent();
        }

        /// <summary>
        /// Desativa um usuário em toda a plataforma, independentemente dos comércios vinculados.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="204">Usuário desativado com sucesso.</response>
        /// <response code="401">Apenas administradores globais podem utilizar esse endpoint.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpDelete("NoComercioId/{id}")]
        [SwaggerOperation(Summary = "Desativar usuário (global)", Description = "Desativa (soft delete) um usuário em toda a plataforma, independentemente dos vínculos com comércios. Restrito a administradores globais (Usuario.Admin == true).")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DesativarGlobal(int id)
        {
            var usuarioIdLogado = GetUsuarioId();
            await _usuarioService.DesativarGlobalAsync(id, usuarioIdLogado);
            return NoContent();
        }

        /// <summary>
        /// Reseta a senha de um usuário.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <param name="request">Nova senha a ser definida.</param>
        /// <response code="204">Senha redefinida com sucesso.</response>
        /// <response code="401">Apenas administradores globais podem utilizar esse endpoint.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpPut("NoComercioId/{id}/Senha")]
        [SwaggerOperation(Summary = "Resetar senha (suporte)", Description = "Redefine a senha de um usuário para uso em atendimento de suporte. Restrito a administradores globais (Usuario.Admin == true).")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResetarSenha(int id, [FromBody] ResetarSenhaRequest request)
        {
            var usuarioIdLogado = GetUsuarioId();
            await _usuarioService.ResetarSenhaAsync(id, usuarioIdLogado, request);
            return NoContent();
        }

        /// <summary>
        /// Concede ou revoga o acesso de administrador de plataforma de um usuário.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <param name="request">Novo status de administrador de plataforma.</param>
        /// <response code="200">Status atualizado com sucesso.</response>
        /// <response code="400">Tentativa de remover o último administrador de plataforma.</response>
        /// <response code="401">Apenas administradores globais podem utilizar esse endpoint.</response>
        /// <response code="404">Usuário não encontrado.</response>
        [HttpPut("NoComercioId/{id}/Admin")]
        [SwaggerOperation(Summary = "Conceder/revogar administrador de plataforma", Description = "Concede ou revoga o acesso de administrador de plataforma (Usuario.Admin) de um usuário, independente dos comércios vinculados. Restrito a administradores globais.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AtualizarAdmin(int id, [FromBody] AtualizarAdminRequest request)
        {
            var usuarioIdLogado = GetUsuarioId();
            var usuario = await _usuarioService.AtualizarAdminAsync(id, usuarioIdLogado, request);
            return Ok(usuario);
        }
    }
}
