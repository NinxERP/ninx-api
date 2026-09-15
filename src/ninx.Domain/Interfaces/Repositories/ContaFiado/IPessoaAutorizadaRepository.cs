using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IPessoaAutorizadaRepository : IRepositoryBase<PessoaAutorizada>
    {
        Task<List<PessoaAutorizada>> GetPorClienteAsync(int clienteId);
        Task<PessoaAutorizada?> GetDoClienteAsync(int pessoaAutorizadaId, int clienteId);
    }
}
