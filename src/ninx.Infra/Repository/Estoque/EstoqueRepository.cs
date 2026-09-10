using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class EstoqueRepository : RepositoryBase<Estoque>, IEstoqueRepository
    {
        private readonly NinxDB _context;
        public EstoqueRepository(NinxDB context) : base(context) 
        {
            _context = context;
        }

        public async Task<Estoque?> GetByProdutoIdAsync(int produtoId, int comercioId)
        {
            return await _context.Estoques
                .FirstOrDefaultAsync(e => e.ProdutoID == produtoId && e.ComercioID == comercioId);
        }
        public async Task<IEnumerable<Estoque>> GetByProdutosIdsAsync(IEnumerable<int> produtoIds, int comercioId)
        {
            return await _context.Estoques
                .Where(e => produtoIds.Contains(e.ProdutoID) && e.ComercioID == comercioId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Estoque> Data, int TotalCount)> GetByComercioIdPaginatedAsync(int comercioId, int pageNumber, int pageSize)
        {
            var query = _context.Estoques
                .AsNoTracking()
                .Where(e => e.ComercioID == comercioId);

            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }
    }
}
