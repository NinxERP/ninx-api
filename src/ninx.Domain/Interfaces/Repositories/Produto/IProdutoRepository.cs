using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories.Base;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IProdutoRepository : IRepositoryBase<Produto>
    {
        Task<IEnumerable<Produto>> GetAllByComercioAsync(int comercioId);
        Task<Produto?> GetByCodigoBarrasAsync(int comercioId, string codigoBarras);
        Task<IEnumerable<Produto>> GetByNomeAsync(int comercioId, string nome);
    }
}