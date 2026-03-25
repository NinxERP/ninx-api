using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Domain.Interfaces.Repositories;
using ninx.Domain.Interfaces.Services;

namespace ninx.Application.Services
{
    public class TrocarComercioService : ITrocarComercioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IJwtTokenService _jwtTokenService;
        public TrocarComercioService
        (
        IUsuarioRepository usuarioRepository,
        IJwtTokenService jwtTokenService
        )
        {
            _usuarioRepository = usuarioRepository;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<string?> TrocarAsync (TrocarComercioRequest request)
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
