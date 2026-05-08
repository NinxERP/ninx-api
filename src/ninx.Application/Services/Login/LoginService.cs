using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class LoginService : ILoginService
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        public LoginService(ITokenProvider tokenProvider, IUsuarioRepository usuarioRepository, IUsuarioComercioRepository usuarioComercioRepository)
        {
            _tokenProvider = tokenProvider;
            _usuarioRepository = usuarioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
        }   

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var usuario = await _usuarioRepository.GetUsuarioByEmail(request.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                throw new BadRequestException("E-mail ou senha incorretos");
            }

            var usuarioComercios = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuario.UsuarioID);
            if (usuarioComercios == null || !usuarioComercios.Any())
            {
                throw new ForbiddenException("Este usuário não possui nenhum comércio vinculado.");
            }

            if (request.ComercioID.HasValue && request.ComercioID > 0)
            {
                var usuarioC = usuarioComercios.FirstOrDefault(x => x.ComercioID == request.ComercioID);
                if (usuarioC == null)
                    throw new UnauthorizedException("Acesso negado ao comércio selecionado.");

                return new LoginResponse { Token = _tokenProvider.GerarToken(usuario, usuarioC.ComercioID, usuarioC.Permissao) };
            }

            if (usuarioComercios.Count() == 1)
            {
                var unico = usuarioComercios.First();
                return new LoginResponse { Token = _tokenProvider.GerarToken(usuario, unico.ComercioID, unico.Permissao) };
            }
            else
            {
                return new LoginResponse
                {
                    Comercios = usuarioComercios.Select(x => new ComercioSimplificado
                    {
                        ComercioID = x.ComercioID,
                        Nome = x.Comercio.NomeComercio
                    }).ToList()
                };
            }
        }
    }
}
