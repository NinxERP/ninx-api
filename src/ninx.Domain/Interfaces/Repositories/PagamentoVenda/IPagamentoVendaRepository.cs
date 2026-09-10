using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IPagamentoVendaRepository : IRepositoryBase<PagamentoVenda>
    {
        Task<IEnumerable<PagamentoVenda>> GetByClienteId(int ClientID);
    }
}
