using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IPagamentoHistoricoAssinaturaPlanoRepository : IRepositoryBase<PagamentoHistoricoAssinaturaPlano>
    {
        Task<PagamentoHistoricoAssinaturaPlano?> GetUltimoPagamentoByAssinaturaPlanoIdAsync(int assinaturaId);
        Task<(IEnumerable<PagamentoHistoricoAssinaturaPlano> Data, int TotalCount)> GetPaginadoByComercioIdAsync(int comercioId, int pageNumber, int pageSize);
    }
}
