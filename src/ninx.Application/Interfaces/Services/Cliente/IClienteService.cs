using ninx.Communication.Request;
using ninx.Communication.Response;

namespace ninx.Application.Interfaces.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteResponse>> GetAll();
        Task<IEnumerable<ClienteResponse>> GetByClienteId(int clienteId);
        Task<ClienteResponse> CriarAsync(ClienteRequest request);
        Task<ClienteResponse> AtualizarAsync(int id, int usuarioLogadoId, ClienteRequest request);
        Task DesativarAsync(int id);
        Task<ClienteResponse> GetByIdAsync(int id);
    }
}
