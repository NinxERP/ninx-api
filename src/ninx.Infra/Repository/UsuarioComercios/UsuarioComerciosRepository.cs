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
                .Include(x => x.Cargo).ThenInclude(c => c.CargoPermissoes).ThenInclude(cp => cp.Permissao)
                .Where(x => x.UsuarioID == usuarioId)
                .ToListAsync();
        }

        public async Task<IEnumerable<UsuarioComercio>> GetByComercioIdAsync(int comercioId)
        {
            return await _context.UsuarioComercio
                .AsNoTracking()
                .Include(x => x.Cargo).ThenInclude(c => c.CargoPermissoes).ThenInclude(cp => cp.Permissao)
                .Where(x => x.ComercioID == comercioId)
                .ToListAsync();
        }

        public async Task<bool> ExisteVinculoAsync(int usuarioId, int comercioId)
        {
            return await _context.UsuarioComercio
                .AsNoTracking()
                .AnyAsync(x => x.UsuarioID == usuarioId && x.ComercioID == comercioId);
        }

        // Para operar no comércio, o vínculo precisa estar ativo. ExisteVinculoAsync, acima, também
        // enxerga vínculos desativados, porque é o que o cadastro precisa para não duplicá-los.
        public async Task<bool> ExisteVinculoAtivoAsync(int usuarioId, int comercioId)
        {
            return await _context.UsuarioComercio
                .AsNoTracking()
                .AnyAsync(x => x.UsuarioID == usuarioId && x.ComercioID == comercioId && x.Ativo);
        }

        public async Task<UsuarioComercio?> GetVinculoAsync(int usuarioId, int comercioId)
        {
            return await _context.UsuarioComercio
                .Include(x => x.Cargo).ThenInclude(c => c.CargoPermissoes).ThenInclude(cp => cp.Permissao)
                .FirstOrDefaultAsync(x => x.UsuarioID == usuarioId && x.ComercioID == comercioId);
        }
    }
}