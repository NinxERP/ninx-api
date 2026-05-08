using ninx.Domain;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services.TrocarComercio
{
    public class TrocarComercioService : ITrocarComercioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenProvider _tokenProvider;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        public TrocarComercioService (IUsuarioRepository usuarioRepository, ITokenProvider tokenProvider, IUsuarioComercioRepository usuarioComercioRepository)
        {
            _usuarioRepository = usuarioRepository;
            _tokenProvider = tokenProvider;
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

            return _tokenProvider.GerarToken(usuario, vinculoNoNovoComercio.ComercioID, vinculoNoNovoComercio.Permissao);
        }
    }
}
