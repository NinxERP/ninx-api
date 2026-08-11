using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IVendaRepository : IRepositoryBase<Venda>
    {
        Task<IEnumerable<Venda>> GetVendasFiltroAsync(DateTime? inicio, DateTime? fim, int comercioID, int? usuarioID);
        Task<IEnumerable<Venda>> GetVendasByUsuarioIdAsync(int usuarioId, int comercioId);
        Task<Venda?> GetByIdAsync(int id);
        Task<Venda?> GetByIdParaEstornoAsync(int id);
        Task<Venda?> GetByIdParaPagamentoFiadoAsync(int id);
        Task<Venda?> GetByIdParaDetalheAsync(int id);
        Task<IEnumerable<Venda>> GetVendasFiadoByClienteIDAsync(int? clienteId);
        Task<IEnumerable<Venda>> GetVendasByClienteIdAsync(int clienteId, int comercioId);
        Task<IEnumerable<Venda>> GetVendasFiadoAtivasPorClienteAsync(int clienteId);
        Task<Dictionary<int, decimal>> GetSaldoDevedorClientesPorComercio(int comercioId);
    }
}
