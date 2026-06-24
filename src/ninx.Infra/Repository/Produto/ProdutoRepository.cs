using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class ProdutoRepository : RepositoryBase<Produto>, IProdutoRepository
    {
        private readonly NinxDB _context;

        public ProdutoRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Produto>> GetProdutosEstoqueByComercioIdAsync(int comercioId)
        {
            return await _context.Produtos
                .Include(x => x.Estoque)
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId)
                .ToListAsync();
        }

        public async Task<(IEnumerable<Produto> Data, int TotalCount)> GetProdutosEstoqueByComercioIdPaginatedAsync(int comercioId, int pageNumber, int pageSize, string? tipoFiltro)
        {
            var query = _context.Produtos
                .Include(x => x.Estoque)
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId);

            if (!string.IsNullOrEmpty(tipoFiltro))
            {
                switch (tipoFiltro.ToLower())
                {
                    case "normal":
                        query = query.Where(x => x.Ativo && x.Estoque.Quantidade >= x.Estoque.QuantidadeMinima);
                        break;
                    case "abaixominimo":
                        query = query.Where(x => x.Ativo && x.Estoque.Quantidade < x.Estoque.QuantidadeMinima && x.Estoque.Quantidade > 0);
                        break;
                    case "semestoque":
                        query = query.Where(x => x.Ativo && x.Estoque.Quantidade == 0);
                        break;
                    case "desativados":
                        query = query.Where(x => !x.Ativo);
                        break;
                }
            }

            var totalCount = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Nome)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<Produto?> GetProdutoByIdAsync(int produtoId)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Where(x => x.ProdutoID == produtoId)
                .FirstOrDefaultAsync();

        }
        public async Task<Produto?> GetAtivosByCodigoBarrasAsync(int comercioId, string codigoBarras)
        {
            return await _context.Produtos
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ComercioID == comercioId && x.CodigoBarras == codigoBarras && x.Ativo == true);
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