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
        /// <response code="404">Usuário não encontrado.</response>
        [HttpGet("NoComercioId/{id}")]
        [SwaggerOperation(Summary = "Buscar usuário por id (sem filtro de comércio)", Description = "Retorna um usuário pelo identificador, sem restringir a busca ao comércio ativo na sessão.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var usuarioIdLogado = GetUsuarioId();
            var usuario = await _usuarioService.GetById(id, usuarioIdLogado);
            return Ok(usuario);
        }

        /// <summary>
        /// Lista os usuários visíveis ao usuário autenticado, sem restringir pelo comércio.
        /// </summary>
        /// <param name="request">Parâmetros de paginação e busca.</param>
        /// <response code="200">Usuários retornados com sucesso.</response>
        /// <response code="404">Nenhum usuário encontrado.</response>
        [HttpGet("NoComercioId/All")]
        [SwaggerOperation(Summary = "Listar usuários (sem filtro de comércio)", Description = "Retorna os usuários visíveis ao usuário autenticado, sem restringir ao comércio ativo na sessão.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll(PaginationRequest request)
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
        /// <response code="404">Nenhum usuário encontrado.</response>
        [HttpGet("All")]
        [SwaggerOperation(Summary = "Listar usuários do comércio", Description = "Retorna os usuários vinculados ao comércio autenticado.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllByComercioId(PaginationRequest request)
        {
            var comercioId = GetComercioId();
            var usuario = await _usuarioService.GetAllByComercioId(comercioId, request);
            return Ok(usuario);
        }

        /// <summary>
        /// Busca um usuário do comércio autenticado pelo identificador.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="200">Usuário encontrado.</response>
        /// <response code="404">Usuário não encontrado no comércio autenticado.</response>
        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Buscar usuário por id", Description = "Retorna um usuário do comércio autenticado pelo identificador.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByIdAndComercioIdAsync(int id)
        {
            var comercioId = GetComercioId();
            var usuario = await _usuarioService.GetByIdAndComercioIdAsync(id, comercioId);
            return Ok(usuario);
        }


        /// <summary>
        /// Cadastra um novo usuário no comércio autenticado.
        /// </summary>
        /// <param name="request">Dados do usuário a ser criado.</param>
        /// <response code="201">Usuário criado com sucesso.</response>
        /// <response code="400">Dados inválidos.</response>
        [HttpPost]
        [SwaggerOperation(Summary = "Criar usuário", Description = "Cadastra um novo usuário vinculado ao comércio autenticado, respeitando a permissão de quem está criando.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
        /// <response code="404">Usuário não encontrado no comércio autenticado.</response>
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar usuário", Description = "Atualiza os dados de um usuário do comércio autenticado.")]
        [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarUsuarioRequest request)
        {
            var comercioId = GetComercioId();
            var usuario = await _usuarioService.AtualizarAsync(id, request, comercioId);
            return Ok(usuario);
        }

        /// <summary>
        /// Desativa um usuário.
        /// </summary>
        /// <param name="id">Identificador do usuário.</param>
        /// <response code="204">Usuário desativado com sucesso.</response>
        /// <response code="404">Usuário não encontrado no comércio autenticado.</response>
        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Desativar usuário", Description = "Desativa (soft delete) um usuário do comércio autenticado.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Desativar(int id)
        {
            var comercioId = GetComercioId();
            await _usuarioService.DesativarAsync(id, comercioId);
            return NoContent();
        }
    }
}
