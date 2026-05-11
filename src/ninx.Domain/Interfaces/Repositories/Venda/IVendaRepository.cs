using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IVendaRepository : IRepositoryBase<Venda>
    {
        Task<IEnumerable<Venda>> GetVendasFiltroAsync(DateTime? inicio, DateTime? fim, int? comercioID, int? usuarioID);
        Task<IEnumerable<Venda>> GetVendasByUsuarioIdAsync(int usuarioId);
        Task<Venda?> GetByIdAsync(int id);
        Task<Venda?> GetByIdComItensAsync(int id);
        Task<IEnumerable<Venda>> GetVendasByClienteIDAsync(int? clienteId);
        Task<IEnumerable<Venda>> GetVendasFiadoByClienteIDAsync(int? clienteId);
    }
}
