using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IProdutoRepository : IRepositoryBase<Produto>
    {
        Task<IEnumerable<Produto>> GetProdutosByComercioIdAsync(int comercioId);
        Task<Produto?> GetByCodigoBarrasAsync(int comercioId, string codigoBarras);
        Task<IEnumerable<Produto>> GetByNomeAsync(int comercioId, string nome);
        Task<Produto?> GetProdutoEstoqueByIdAsync(int produtoId);
        Task<IEnumerable<Produto>> GetProdutosById(IEnumerable<int> produtoIds);
        Task<Produto?> GetByIdAndComercioAsync(int id, int comercioId);
    }
}