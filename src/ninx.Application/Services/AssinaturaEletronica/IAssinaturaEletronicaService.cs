using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IAssinaturaEletronicaService
    {
        Task<IEnumerable<AssinaturaEletronicaResponse>> GetAll();
        Task<AssinaturaEletronicaResponse> GetByIdAsync(int id);

        Task<AssinaturaEletronicaResponse> ObterDadosParaAssinaturaAsync(Guid guid);
        Task<AssinaturaEletronicaResponse> ObterDocumentoAssinadoAsync(Guid guid, int comercioId);

        Task ConfirmarAssinaturaAsync(Guid guid, string imagemBase64, string ip, string dispositivo);
        Task<bool> ValidaAssinado(Guid guid);
    }
}