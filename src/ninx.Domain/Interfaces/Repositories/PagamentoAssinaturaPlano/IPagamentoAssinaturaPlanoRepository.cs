using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IPagamentoHistoricoAssinaturaPlanoRepository : IRepositoryBase<PagamentoHistoricoAssinaturaPlano>
    {
        Task<PagamentoHistoricoAssinaturaPlano?> GetUltimoPagamentoByAssinaturaPlanoIdAsync(int assinaturaId);
    }
}
