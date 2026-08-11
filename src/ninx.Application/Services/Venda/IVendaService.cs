using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IVendaService
    {
        Task<IEnumerable<VendaResponse>> GetVendasFiltroAsync(FiltroRequest request, int comercioId);
        Task<IEnumerable<VendaResponse>> GetByUsuarioIdAsync(int usuarioID, int comercioId);
        Task<VendaResponse> GetByVendaIdAsync(int id, int comercioId);
        Task<VendaResponse> CriarAsync(CriarVendaRequest request);
        Task EstornarAsync(int vendaId, int usuarioId);
        Task<Guid> ReceberPagamentoFiadoAsync(int vendaId, int usuarioId, decimal valorPago, int formaPagamento);
        Task<IEnumerable<VendaResponse>> GetByClienteIdAsync(int clienteId, int comercioId);
        Task<Guid> ReceberPagamentoGeralFiadoAsync(int clienteId, int usuarioId, decimal valorTotalPago, int formaPagamento);
    }
}
