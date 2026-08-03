using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IProdutoRepository : IRepositoryBase<Produto>
    {
        Task<IEnumerable<Produto>> GetProdutosEstoqueByComercioIdAsync(int comercioId);
        Task<Produto?> GetAtivosByCodigoBarrasAsync(int comercioId, string codigoBarras);
        Task<IEnumerable<Produto>> GetByNomeAsync(int comercioId, string nome);
        Task<Produto?> GetProdutoByIdAsync(int produtoId);
        Task<IEnumerable<Produto>> GetProdutosById(IEnumerable<int> produtoIds);
        Task<Produto?> GetByIdAndComercioAsync(int id, int comercioId);
        Task<(IEnumerable<Produto> Data, int TotalFiltrado, MetricsSummary Metrics)> GetProdutosEstoqueByComercioIdPaginatedAsync(int comercioId, PaginationRequest request);
    }
}