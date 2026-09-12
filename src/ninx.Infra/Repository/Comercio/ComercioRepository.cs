using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class ComercioRepository : RepositoryBase<Comercio>, IComercioRepository
    {
        private readonly NinxDB _context;
        public ComercioRepository(NinxDB context) : base(context)
        {
            _context = context;
        }
            public async Task<IEnumerable<Comercio>> GetByUsuarioId(int usuarioId)
        {
            return await _context.Comercio
                .Include(c => c.UsuarioComercios)
                .Where(c => c.UsuarioComercios
                .Any(uc => uc.UsuarioID == usuarioId))
                .ToListAsync();
        }

        public async Task<(IEnumerable<Comercio> Data, int TotalCount)> GetPaginatedAsync(int pageNumber, int pageSize, string? termoBusca)
        {
            var query = _context.Comercio.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(termoBusca))
                query = query.Where(c => c.NomeComercio.Contains(termoBusca));

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(c => c.NomeComercio)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
