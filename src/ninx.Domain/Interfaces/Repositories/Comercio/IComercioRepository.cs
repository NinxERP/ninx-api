using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IComercioRepository : IRepositoryBase<Comercio>
    {
        public Task<IEnumerable<Comercio>> GetByUsuarioId(int usuarioId);
    }
}
