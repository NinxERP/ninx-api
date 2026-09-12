using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IPermissaoRepository : IRepositoryBase<Permissao>
    {
        Task<IEnumerable<Permissao>> GetTodasAsync();
        Task<IEnumerable<Permissao>> GetByIdsAsync(IEnumerable<int> ids);
    }
}
