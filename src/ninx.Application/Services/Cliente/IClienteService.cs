using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IClienteService
    {
        Task<PaginatedResponse<ClienteResponse>> GetAllByComercioId(int comercioId, int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<ClienteResponse>> GetByIdAsync(int clienteId, int comercioId);
        Task<ClienteResponse> CriarAsync(ClienteRequest request, int comercioId);
        Task<ClienteResponse> AtualizarAsync(int id, int usuarioLogadoId, ClienteRequest request, int comercioId);
        Task DesativarAsync(int id, int comercioId);
    }
}
