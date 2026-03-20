using ninx.Communication.Request;
using ninx.Domain.Interfaces.Repositories.Usuario;
using ninx.Domain.Interfaces.Services.JwtToken;
using BCrypt.Net;
using ninx.Domain.Interfaces.Services;
using ninx.Application.Interfaces.Services.Login;
namespace ninx.Application.Services.Login
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

        public async Task<string?> LoginAsync(LoginRequest request)
        {
            var usuario = await _usuarioRepository.GetUsuarioAndUsuarioComercioByEmail(request.Email);
            if (usuario == null)
            {
                return null;
            }
            if (!BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            {
                return null;
            }
            var uComercio = usuario.UsuarioComercios.FirstOrDefault(x => x.ComercioID == request.ComercioID);
            if (uComercio == null)
            {
                return null; 
            }
            return _jwtTokenService.GerarToken(usuario, request.ComercioID);
        }
    }
}
