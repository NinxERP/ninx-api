using ninx.Communication;
using ninx.Communication.Response;

namespace ninx.Application.Services
{
    public interface ICategoriaProdutoService
    {
        Task<PaginatedResponse<CategoriaProdutoResponse>> GetAllAsync(int comercioId, PaginationRequest requst);
        Task<CategoriaProdutoResponse> GetByIdAsync(int categoriaId, int comercioId);
        Task<CategoriaProdutoResponse> CreateAsync(CategoriaProdutoRequest request, int comercioId);
        Task<CategoriaProdutoResponse> UpdateAsync(int categoriaId, CategoriaProdutoRequest request, int comercioId);
        Task DeleteAsync(int categoriaId, int comercioId);
    }
}
