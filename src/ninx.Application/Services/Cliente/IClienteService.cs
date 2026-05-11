using ninx.Communication;
using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IClienteService
    {
        Task<IEnumerable<ClienteResponse>> GetAllByComercioId(int comercioId);
        Task<IEnumerable<ClienteResponse>> GetByIdAsync(int clienteId, int comercioId);
        Task<ClienteResponse> CriarAsync(ClienteRequest request, int comercioId);
        Task<ClienteResponse> AtualizarAsync(int id, int usuarioLogadoId, ClienteRequest request, int comercioId);
        Task DesativarAsync(int id, int comercioId);
    }
}
