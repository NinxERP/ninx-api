using ninx.Communication.Request;
using ninx.Communication.Response;

namespace ninx.Application.Services
{
    public interface IVendaService
    {
        Task<IEnumerable<VendaResponse>> GetVendasFiltroAsync(FiltroRequest request);
        Task<IEnumerable<VendaResponse>> GetByUsuarioIdAsync(int usuarioID);
        Task<VendaResponse> GetByVendaIdAsync(int id);
        Task<VendaResponse> CriarAsync(CriarVendaRequest request);
        Task EstornarAsync(int vendaId, int usuarioId);
        Task ReceberPagamentoFiadoAsync(int vendaId, int usuarioId, decimal valorPago, int formaPagamento);
    }
}
