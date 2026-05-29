using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IClienteRepository : IRepositoryBase<Cliente>
    {
        Task<List<Cliente>> GetByNomeAsync(string nome, int comercioId);
    }
}
