using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IEstoqueService
    {
        Task<PaginatedResponse<EstoqueResponse>> GetAllByComercioIdAsync(int comercioId, int pageNumber = 1, int pageSize = 10);
        Task<EstoqueResponse> GetByIdAsync(int estoqueId);
        Task<EstoqueResponse> CreateAsync(EstoqueRequest request, int comercioId);
        Task<EstoqueResponse> UpdateAsync(int estoqueId, EstoqueRequest request, int comercioId);
        Task DeleteAsync(int estoqueId, int comercioId);
    }
}
