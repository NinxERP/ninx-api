using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class LogAuditoriaRepository : RepositoryBase<LogAuditoria>, ILogAuditoriaRepository
    {
        private readonly NinxDB _context;

        public LogAuditoriaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<LogAuditoria> Data, int TotalCount)> GetPaginatedFiltradoAsync(int? comercioId, int? usuarioId, int pageNumber, int pageSize)
        {
            var query = _context.LogsAuditoria.AsNoTracking().Include(x => x.Usuario).AsQueryable();

            if (comercioId.HasValue)
                query = query.Where(x => x.ComercioID == comercioId);

            if (usuarioId.HasValue)
                query = query.Where(x => x.UsuarioID == usuarioId);

            query = query.OrderByDescending(x => x.CriadoEm);

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
