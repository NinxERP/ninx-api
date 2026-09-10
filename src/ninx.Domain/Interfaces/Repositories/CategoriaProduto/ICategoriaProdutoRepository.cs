using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface ICategoriaProdutoRepository : IRepositoryBase<CategoriaProduto>
    {
        Task<CategoriaProduto?> GetByIdAndComercioIdAsync(int categoriaId, int comercioId);
        Task<(IEnumerable<CategoriaProduto> Data, int TotalCount)> GetByComercioIdPaginatedAsync(int comercioId, int pageNumber, int pageSize);
        Task<bool> ExisteProdutoVinculadoAsync(int categoriaId);
    }
}
