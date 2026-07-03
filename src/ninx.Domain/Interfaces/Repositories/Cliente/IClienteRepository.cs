using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IClienteRepository : IRepositoryBase<Cliente>
    {
        Task<List<Cliente>> GetByNomeAsync(string nome, int comercioId);
        Task<(IEnumerable<Cliente> Data, int TotalCount, int TotalAtivos)> GetClienteComercioByComercioId(int comercioId, PaginationRequest request);
    }
}
