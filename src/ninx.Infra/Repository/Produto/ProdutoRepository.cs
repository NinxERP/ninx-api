using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class ProdutoRepository : RepositoryBase<Produto>, IProdutoRepository
    {
        private readonly NinxDB _context;

        public ProdutoRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Produto>> GetProdutosByComercioIdAsync(int comercioId)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId)
                .ToListAsync();
        }
        public async Task<Produto?> GetProdutoByIdAsync(int produtoId)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Where(x => x.ProdutoID == produtoId)
                .FirstOrDefaultAsync();

        }
        public async Task<Produto?> GetByCodigoBarrasAsync(int comercioId, string codigoBarras)
        {
            return await _context.Produtos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ComercioID == comercioId && x.CodigoBarras == codigoBarras);
        }

        public async Task<IEnumerable<Produto>> GetByNomeAsync(int comercioId, string nome)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId && x.Nome.Contains(nome))
                .ToListAsync();
        }
        public async Task<Produto?> GetByIdAndComercioAsync(int id, int comercioId)
        {
            return await _context.Produtos
                .FirstOrDefaultAsync(p => p.ProdutoID == id && p.ComercioID == comercioId);
        }
        public async Task<IEnumerable<Produto>> GetProdutosById(IEnumerable<int> produtoIds)
        {
            return await _context.Produtos
                .Include(x => x.Estoque)
                .Where(x => produtoIds.Contains(x.ProdutoID))
                .ToListAsync();
        }
    }
}