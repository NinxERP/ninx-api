using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class PagamentoHistoricoAssinaturaPlanoRepository : RepositoryBase<PagamentoHistoricoAssinaturaPlano>, IPagamentoHistoricoAssinaturaPlanoRepository
    {
        private readonly NinxDB _context;
        public PagamentoHistoricoAssinaturaPlanoRepository(NinxDB context) : base(context)
        {
            _context = context;
        }


        public async Task<PagamentoHistoricoAssinaturaPlano?> GetUltimoPagamentoByAssinaturaPlanoIdAsync(int assinaturaId)
        {
            return await _context.PagamentoHistoricoAssinaturaPlano
                .Where(x => x.AssinaturaID == assinaturaId)
                .OrderByDescending(x => x.DataPagamento)
                .FirstOrDefaultAsync();
        }
    }
}
