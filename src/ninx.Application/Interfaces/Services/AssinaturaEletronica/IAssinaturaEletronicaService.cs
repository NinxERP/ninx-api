using ninx.Communication.Response;

namespace ninx.Application.Interfaces.Services
{
    public interface IAssinaturaEletronicaService
    {
        Task<IEnumerable<AssinaturaEletronicaResponse>> GetAll();
        Task<AssinaturaEletronicaResponse> GetByIdAsync(int id);

        Task<AssinaturaEletronicaResponse> ObterDadosParaAssinaturaAsync(Guid guid);

        Task ConfirmarAssinaturaAsync(Guid guid, string imagemBase64, string ip, string dispositivo);
    }
}