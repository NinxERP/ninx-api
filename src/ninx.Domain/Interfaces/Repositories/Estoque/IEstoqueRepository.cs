using ninx.Domain.Entities;
namespace ninx.Domain.Interfaces
    
{
    public interface IEstoqueRepository : IRepositoryBase<Estoque>
    {
        Task<Estoque?> GetByProdutoIdAsync(int produtoId, int comercioId);
        Task<IEnumerable<Estoque>> GetByProdutosIdsAsync(IEnumerable<int> produtoIds, int comercioId);
    }
}
