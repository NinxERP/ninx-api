using ninx.Domain.Interfaces.Repositories.Base;

namespace ninx.Domain.Interfaces.Repositories.Comercio
{
    public interface IComercioRepository : IRepositoryBase<Entities.Comercio>
    {
        public Task<IEnumerable<Domain.Entities.Comercio>> GetByUsuarioId(int usuarioId);
    }
}
