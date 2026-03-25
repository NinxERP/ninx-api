using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class VendaRepository : RepositoryBase<Venda>, IVendaRepository
    {
        private readonly NinxDB _context;

        public VendaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venda>> GetVendasFiltroAsync(DateTime? inicio, DateTime? fim, int? comercioID, int? usuarioID)
        {
            var query = _context.Vendas.AsQueryable();

            if (inicio.HasValue)
                query = query.Where(v => v.CriadoEm >= inicio.Value);

            if (fim.HasValue)
                query = query.Where(v => v.CriadoEm <= fim.Value);

            if (usuarioID.HasValue)
                query = query.Where(v => v.UsuarioID == usuarioID.Value);

            if (comercioID.HasValue)
                query = query.Where(v => v.ComercioID == comercioID.Value);

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<Venda>> GetVendasByUsuarioIdAsync(int usuarioId)
        {
            return await _context.Vendas
                .AsNoTracking()
                .Where(v => v.UsuarioID == usuarioId)
                .ToListAsync();
        }

    }
}
