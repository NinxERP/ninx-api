using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class AssinaturaPlanoRepository : RepositoryBase<AssinaturaPlano>, IAssinaturaPlanoRepository
    {
        private readonly NinxDB _context;

        public AssinaturaPlanoRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<AssinaturaPlano?> GetByComercioIdAsync(int comercioId)
        {
            return await _context.AssinaturaPlano
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ComercioID == comercioId);
        }
    }
}
