using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;
using ninx.Domain.Interfaces.Services;

namespace ninx.Application.Services
{
    public class TrocarComercioService : ITrocarComercioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        public TrocarComercioService (IUsuarioRepository usuarioRepository, IJwtTokenService jwtTokenService, IUsuarioComercioRepository usuarioComercioRepository)
        {
            _usuarioRepository = usuarioRepository;
            _jwtTokenService = jwtTokenService;
            _usuarioComercioRepository = usuarioComercioRepository;
        }

        public async Task<string> TrocarAsync(int comercioID, int usuarioID)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioID);
            if (usuario == null)
                throw new NotFoundException("Usuário não encontrado.");

            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuario.UsuarioID);
            var vinculoNoNovoComercio = usuarioComercio
                .FirstOrDefault(x => x.ComercioID == comercioID);

            if (vinculoNoNovoComercio == null)
            {
                throw new UnauthorizedException("Você não tem acesso a este comércio.");
            }

            return _jwtTokenService.GerarToken(usuario, vinculoNoNovoComercio.ComercioID, vinculoNoNovoComercio.Permissao);
        }
    }
}
