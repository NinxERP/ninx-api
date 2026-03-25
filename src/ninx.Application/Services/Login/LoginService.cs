using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;
using ninx.Domain.Interfaces.Services;

namespace ninx.Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUsuarioRepository _usuarioRepository;
        public LoginService(IJwtTokenService jwtTokenService, IUsuarioRepository usuarioRepository)
        {
            _jwtTokenService = jwtTokenService;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var usuario = await _usuarioRepository.GetUsuarioAndUsuarioComercioByEmail(request.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                throw new BadRequestException("E-mail ou senha incorretos");
            }

            if (request.ComercioID != null && request.ComercioID > 0)
            {
                var temAcesso = usuario.UsuarioComercios.Any(x => x.ComercioID == request.ComercioID);
                if (!temAcesso)
                {
                    throw new BadRequestException("Usuário não tem comercios vinculados");
                }
                return new LoginResponse
                {
                    Token = _jwtTokenService.GerarToken(usuario, request.ComercioID)
                };
            }

            if (usuario.UsuarioComercios.Count == 1)
            {
                return new LoginResponse
                {
                    Token = _jwtTokenService.GerarToken(usuario, usuario.UsuarioComercios.First().ComercioID)
                };
            }

            if (usuario.UsuarioComercios.Count > 1)
            {
                return new LoginResponse
                {
                    Comercios = usuario.UsuarioComercios.Select(x => new ComercioSimplificado
                    {
                        ComercioID = x.ComercioID,
                        Nome = x.Comercio.Nome
                    }).ToList()
                };
            }
            return null;
        }
    }
}
