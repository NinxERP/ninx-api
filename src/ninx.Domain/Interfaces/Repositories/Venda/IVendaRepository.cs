using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IVendaRepository : IRepositoryBase<Venda>
    {
        Task<IEnumerable<Venda>> GetVendasFiltroAsync(DateTime? inicio, DateTime? fim, int? comercioID, int? usuarioID);
        Task<IEnumerable<Venda>> GetVendasByUsuarioIdAsync(int usuarioId);
    }
}
