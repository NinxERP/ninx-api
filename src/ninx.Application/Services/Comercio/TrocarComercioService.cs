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

        public async Task<string> TrocarAsync(TrocarComercioRequest request, int usuarioLogadoId)
        {
            if (request.UsuarioId != usuarioLogadoId)
            {
                throw new UnauthorizedException("Você não pode trocar o contexto de outro usuário.");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(request.UsuarioId);
            if (usuario == null)
                throw new NotFoundException("Usuário não encontrado.");

            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuario.UsuarioID);

            var vinculoNoNovoComercio = usuarioComercio
                .FirstOrDefault(x => x.ComercioID == request.NovoComercioId);

            if (vinculoNoNovoComercio == null)
            {
                throw new UnauthorizedException("Você não tem acesso a este comércio.");
            }

            return _jwtTokenService.GerarToken(usuario, vinculoNoNovoComercio.ComercioID);
        }
    }
}
