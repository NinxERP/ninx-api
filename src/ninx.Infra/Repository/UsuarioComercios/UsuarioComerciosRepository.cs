using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class UsuarioComercioRepository : RepositoryBase<UsuarioComercio>, IUsuarioComercioRepository
    {
        private readonly NinxDB _context;

        public UsuarioComercioRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UsuarioComercio>> GetByUsuarioIdAsync(int usuarioId)
        {
            return await _context.UsuarioComercio
                .AsNoTracking()
                .Include(x => x.Comercio)
                .Where(x => x.UsuarioID == usuarioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UsuarioComercio>> GetByComercioIdAsync(int comercioId)
        {
            return await _context.UsuarioComercio
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId)
                .ToListAsync();
        }

        public async Task<bool> ExisteVinculoAsync(int usuarioId, int comercioId)
        {
            return await _context.UsuarioComercio
                .AsNoTracking()
                .AnyAsync(x => x.UsuarioID == usuarioId && x.ComercioID == comercioId);
        }

        public async Task<UsuarioComercio?> GetVinculoAsync(int usuarioId, int comercioId)
        {
            return await _context.UsuarioComercio
                .FirstOrDefaultAsync(x => x.UsuarioID == usuarioId && x.ComercioID == comercioId);
        }
    }
}