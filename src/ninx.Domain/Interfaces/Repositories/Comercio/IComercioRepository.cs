using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IComercioRepository : IRepositoryBase<Comercio>
    {
        public Task<IEnumerable<Comercio>> GetByUsuarioId(int usuarioId);
    }
}
