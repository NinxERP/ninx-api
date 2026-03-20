using ninx.Application.Interfaces.Services.SwitchComercio;
using ninx.Communication.Request;
using ninx.Domain.Interfaces.Repositories.Usuario;
using ninx.Domain.Interfaces.Services.JwtToken;

namespace ninx.Application.Services.SwitchComercio
{
    public class SwitchComercioService : ISwitchComercioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IJwtTokenService _jwtTokenService;
        public SwitchComercioService
        (
        IUsuarioRepository usuarioRepository,
        IJwtTokenService jwtTokenService
        )
        {
            _usuarioRepository = usuarioRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string?> TrocarAsync (SwitchComercioRequest request)
        {
            var usuario = await _usuarioRepository.GetUsuarioAndUsuarioComercioById(request.UsuarioId);
            if (usuario == null)
            {
                return null;
            }

            var temAcesso = usuario.UsuarioComercios.Any(x => x.ComercioID == request.NovoComercioId);
            if (!temAcesso)
            {
                return null;
            }

            return _jwtTokenService.GerarToken(usuario, request.NovoComercioId);
        }
    }
}
