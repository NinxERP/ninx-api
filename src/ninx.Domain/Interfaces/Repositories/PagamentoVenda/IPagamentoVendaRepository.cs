using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IPagamentoVendaRepository : IRepositoryBase<PagamentoVenda>
    {
        Task<IEnumerable<PagamentoVenda>> GetByClienteId(int ClientID);
    }
}
