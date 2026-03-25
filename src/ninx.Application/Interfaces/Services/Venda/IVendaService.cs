using ninx.Communication.Response;

namespace ninx.Application.Interfaces.Services
{
    public interface IVendaService
    {
        Task<IEnumerable<VendaResponse>> GetVendasFiltroAsync(
                DateTime? inicio = null,
                DateTime? fim = null,
                int? comercioID = null,
                int? usuarioId = null);
    }
}
