using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services.TrocarComercio
{
    public class TrocarComercioService : ITrocarComercioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenProvider _tokenProvider;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;
        private readonly IComercioRepository _comercioRepository;
        private readonly ICargoEfetivoService _cargoEfetivoService;

        public TrocarComercioService(
            IUsuarioRepository usuarioRepository,
            ITokenProvider tokenProvider,
            IUsuarioComercioRepository usuarioComercioRepository,
            IComercioRepository comercioRepository,
            ICargoEfetivoService cargoEfetivoService)
        {
            _usuarioRepository = usuarioRepository;
            _tokenProvider = tokenProvider;
            _usuarioComercioRepository = usuarioComercioRepository;
            _comercioRepository = comercioRepository;
            _cargoEfetivoService = cargoEfetivoService;
        }

        public async Task<string> TrocarAsync(int comercioID, int usuarioID)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioID);
            if (usuario == null)
                throw new NotFoundException("Usuário não encontrado.");

            var usuarioComercio = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuario.UsuarioID);
            var vinculoNoNovoComercio = usuarioComercio
                .FirstOrDefault(x => x.ComercioID == comercioID);

            if (vinculoNoNovoComercio != null)
            {
                var cargoEfetivoVinculo = await _cargoEfetivoService.ResolverCargoEfetivoAsync(usuario, vinculoNoNovoComercio.Cargo);
                return _tokenProvider.GerarToken(usuario, vinculoNoNovoComercio.ComercioID, cargoEfetivoVinculo, vinculoNoNovoComercio.Comercio.NomeComercio);
            }

            // Sem vínculo: só administradores de plataforma podem entrar num comércio onde não têm vínculo próprio.
            var comercio = await _comercioRepository.GetByIdAsync(comercioID);
            if (comercio == null || !comercio.Ativo)
                throw new NotFoundException("Comércio não encontrado.");

            var cargoEfetivoAdmin = await _cargoEfetivoService.ResolverCargoEfetivoAsync(usuario, cargoDoVinculo: null);
            return _tokenProvider.GerarToken(usuario, comercio.ComercioID, cargoEfetivoAdmin, comercio.NomeComercio);
        }
    }
}
