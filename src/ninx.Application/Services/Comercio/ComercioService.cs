using ninx.Application.Interfaces.Services.Comercio;
using ninx.Domain.Interfaces.Repositories.Comercio;

namespace ninx.Application.Services.Comercio
{
    public class ComercioService : IComercioService
    {
        public readonly IComercioRepository _comercioRepository;
        public ComercioService(IComercioRepository comercioRepository)
        {
            _comercioRepository = comercioRepository;
        }
        public async Task<IEnumerable<Domain.Entities.Comercio>> GetAll()
        {
            return await _comercioRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Domain.Entities.Comercio>> GetByUsuarioId(int usuarioId)
        {
            return await _comercioRepository.GetByUsuarioId(usuarioId);
        }
    }
}
