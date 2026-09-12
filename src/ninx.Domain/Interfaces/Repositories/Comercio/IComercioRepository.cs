using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IComercioRepository : IRepositoryBase<Comercio>
    {
        public Task<IEnumerable<Comercio>> GetByUsuarioId(int usuarioId);
        public Task<(IEnumerable<Comercio> Data, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize, string? termoBusca);
    }
}
