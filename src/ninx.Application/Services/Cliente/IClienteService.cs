using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IClienteService
    {
        Task<PaginatedResponse<ClienteResponse>> GetAllByComercioId(int comercioId, PaginationRequest request);
        Task<IEnumerable<ClienteResponse>> GetByIdAsync(int clienteId, int comercioId);
        Task<ClienteResponse> CriarAsync(ClienteRequest request, int comercioId);
        Task<ClienteResponse> AtualizarAsync(int id, int usuarioLogadoId, ClienteRequest request, int comercioId);
        Task DesativarAsync(int id, int comercioId);
        Task<IEnumerable<ClienteResponse>> GetByNomeAsync(string Nome, int comercioId);
    }
}
