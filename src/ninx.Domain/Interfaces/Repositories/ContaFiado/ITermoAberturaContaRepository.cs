using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface ITermoAberturaContaRepository : IRepositoryBase<TermoAberturaConta>
    {
        Task<TermoAberturaConta?> GetAtivoAsync(int clienteId);
        Task<List<TermoAberturaConta>> GetAguardandoAsync(int clienteId);
        Task<List<TermoAberturaConta>> GetPorClienteAsync(int clienteId);
    }
}
