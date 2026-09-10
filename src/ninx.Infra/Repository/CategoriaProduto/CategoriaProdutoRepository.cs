using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class CategoriaProdutoRepository : RepositoryBase<CategoriaProduto>, ICategoriaProdutoRepository
    {
        private readonly NinxDB _context;

        public CategoriaProdutoRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<CategoriaProduto?> GetByIdAndComercioIdAsync(int categoriaId, int comercioId)
        {
            return await _context.CategoriaProduto
                .FirstOrDefaultAsync(x => x.CategoriaID == categoriaId && x.ComercioID == comercioId);
        }

        public async Task<(IEnumerable<CategoriaProduto> Data, int TotalCount)> GetByComercioIdPaginatedAsync(int comercioId, int pageNumber, int pageSize)
        {
            var query = _context.CategoriaProduto
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId);

            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<bool> ExisteProdutoVinculadoAsync(int categoriaId)
        {
            return await _context.Produtos.AnyAsync(p => p.CategoriaID == categoriaId);
        }
    }
}
