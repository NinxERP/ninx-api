using ninx.Communication;
using ninx.Communication.Response;

namespace ninx.Application.Services
{
    public interface ICategoriaProdutoService
    {
        Task<IEnumerable<CategoriaProdutoResponse>> GetAllAsync();
        Task<CategoriaProdutoResponse> GetByIdAsync(int categoriaId);
        Task<CategoriaProdutoResponse> CreateAsync(CategoriaProdutoRequest request);
        Task<CategoriaProdutoResponse> UpdateAsync(int categoriaId, CategoriaProdutoRequest request);
        Task DeleteAsync(int categoriaId);
    }
}
