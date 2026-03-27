using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

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
}
