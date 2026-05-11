using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IEstoqueService
    {
        Task<IEnumerable<EstoqueResponse>> GetAllByComercioIdAsync(int comercioId);
        Task<EstoqueResponse> GetByIdAsync(int estoqueId);
        Task<EstoqueResponse> CreateAsync(EstoqueRequest request, int comercioId);
        Task<EstoqueResponse> UpdateAsync(int estoqueId, EstoqueRequest request, int comercioId);
        Task DeleteAsync(int estoqueId, int comercioId);
    }
}
