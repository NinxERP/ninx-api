using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IDocumentosVendaService
    {
        Task<IEnumerable<DocumentosVendaResponse>> GetAll();
        Task<DocumentosVendaResponse> GetByIdAsync(int id);

        Task<DocumentosVendaResponse> ObterDadosParaAssinaturaAsync(Guid guid);

        Task ConfirmarAssinaturaAsync(Guid guid, string imagemBase64, string ip, string dispositivo);
        Task<bool> ValidaAssinado(Guid guid);
    }
}