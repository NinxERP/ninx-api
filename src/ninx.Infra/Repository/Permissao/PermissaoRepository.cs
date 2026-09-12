using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class PermissaoRepository : RepositoryBase<Permissao>, IPermissaoRepository
    {
        private readonly NinxDB _context;

        public PermissaoRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Permissao>> GetTodasAsync()
        {
            return await _context.Permissoes
                .AsNoTracking()
                .OrderBy(x => x.Nome)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permissao>> GetByIdsAsync(IEnumerable<int> ids)
        {
            return await _context.Permissoes
                .AsNoTracking()
                .Where(x => ids.Contains(x.PermissaoID))
                .ToListAsync();
        }
    }
}
