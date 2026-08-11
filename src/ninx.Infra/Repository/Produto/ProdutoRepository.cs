using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using ninx.Communication;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;
using System.Linq;

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

        public async Task<(IEnumerable<Produto> Data, int TotalFiltrado, MetricsSummary Metrics)> GetProdutosEstoqueByComercioIdPaginatedAsync(
            int comercioId,
            PaginationRequest request)
        {
            var queryBase = _context.Produtos
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId);

            var metrics = await queryBase
                .GroupBy(_ => 1)
                .Select(g => new MetricsSummary
                {
                    TotalAtivos = g.Count(x => x.Ativo),
                    TotalNormal = g.Count(x => x.Ativo && x.Estoque.Quantidade >= x.Estoque.QuantidadeMinima),
                    TotalBaixo = g.Count(x => x.Ativo && x.Estoque.Quantidade < x.Estoque.QuantidadeMinima && x.Estoque.Quantidade > 0),
                    TotalZerado = g.Count(x => x.Ativo && x.Estoque.Quantidade == 0)
                })
                .FirstOrDefaultAsync() ?? new MetricsSummary();

            var queryData = queryBase;

            if (!string.IsNullOrWhiteSpace(request.TermoBusca))
            {
                var termo = request.TermoBusca.Trim();
                queryData = queryData.Where(p =>
                    EF.Functions.Like(p.Nome, $"%{termo}%") ||
                    (p.CodigoBarras != null && EF.Functions.Like(p.CodigoBarras, $"%{termo}%")));
            }

            if (request.Status != null && request.Status.Any())
            {
                var filtros = request.Status.Select(s => s.ToLower().Trim()).ToList();

                queryData = queryData.Where(x =>
                    (filtros.Contains("desativados") && !x.Ativo) ||
                    (filtros.Contains("ok") && x.Ativo && x.Estoque.Quantidade >= x.Estoque.QuantidadeMinima) ||
                    (filtros.Contains("baixo") && x.Ativo && x.Estoque.Quantidade < x.Estoque.QuantidadeMinima && x.Estoque.Quantidade > 0) ||
                    (filtros.Contains("zerado") && x.Ativo && x.Estoque.Quantidade == 0)
                );
            }
            else
            {
                queryData = queryData.Where(x => x.Ativo);
            }

            var totalFiltrado = await queryData.CountAsync();

            var data = await queryData
                .Include(x => x.Estoque)
                .OrderBy(x => x.Nome)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (data, totalFiltrado, metrics);
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
                .Include(x => x.Categoria)
                .Include(x => x.Estoque)
                .FirstOrDefaultAsync(x => x.ComercioID == comercioId && x.CodigoBarras == codigoBarras && x.Ativo == true);
        }

        public async Task<IEnumerable<Produto>> GetByNomeAsync(int comercioId, string nome)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Include(x => x.Categoria)
                .Include(x => x.Estoque)
                .Where(x => x.ComercioID == comercioId && x.Nome.Contains(nome))
                .ToListAsync();
        }
        public async Task<Produto?> GetByIdAndComercioAsync(int id, int comercioId)
        {
            return await _context.Produtos
                .Include(x => x.Categoria)
                .Include(x => x.Estoque)
                .FirstOrDefaultAsync(p => p.ProdutoID == id && p.ComercioID == comercioId);
        }
        public async Task<IEnumerable<Produto>> GetProdutosById(IEnumerable<int> produtoIds)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Where(x => produtoIds.Contains(x.ProdutoID))
                .ToListAsync();
        }
    }
}