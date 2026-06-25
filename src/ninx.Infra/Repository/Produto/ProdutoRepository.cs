using Microsoft.EntityFrameworkCore;
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

        public async Task<(IEnumerable<Produto> Data, PaginatedResponse<ProdutoResponse> dadosPaginated)> GetProdutosEstoqueByComercioIdPaginatedAsync(
            int comercioId,
            PaginationRequest request)
        {
            var query = _context.Produtos
                .Include(x => x.Estoque)
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId);

            var totalAtivos = await query.CountAsync(x => x.Ativo);
            var totalNormal = await query.CountAsync(x => x.Ativo && x.Estoque.Quantidade >= x.Estoque.QuantidadeMinima);
            var totalBaixo = await query.CountAsync(x => x.Ativo && x.Estoque.Quantidade < x.Estoque.QuantidadeMinima && x.Estoque.Quantidade > 0);
            var totalZerado = await query.CountAsync(x => x.Ativo && x.Estoque.Quantidade == 0);

            if (!string.IsNullOrWhiteSpace(request.TermoBusca))
            {
                var t = request.TermoBusca.Trim().ToLower();
                query = query.Where(p => p.Nome.ToLower().Contains(t) || (p.CodigoBarras != null && p.CodigoBarras.Contains(t)));
            }

            if (request.Status != null && request.Status.Any())
            {
                var filtros = request.Status.Select(s => s.ToLower().Trim()).ToList();

                query = query.Where(x =>
                    (filtros.Contains("desativados") && !x.Ativo) ||
                    (filtros.Contains("ok") && x.Ativo && x.Estoque.Quantidade >= x.Estoque.QuantidadeMinima) ||
                    (filtros.Contains("baixo") && x.Ativo && x.Estoque.Quantidade < x.Estoque.QuantidadeMinima && x.Estoque.Quantidade > 0) ||
                    (filtros.Contains("zerado") && x.Ativo && x.Estoque.Quantidade == 0)
                );
            }
            else
            {
                query = query.Where(x => x.Ativo);
            }

            var totalRecordsFiltrado = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Nome)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dadosPaginated = new PaginatedResponse<ProdutoResponse>(
                data: new List<ProdutoResponse>(), 
                pageNumber: request.PageNumber,
                pageSize: request.PageSize,
                totalRecords: totalRecordsFiltrado,
                totalAtivos: totalAtivos,
                totalNormal: totalNormal,
                totalBaixo: totalBaixo,
                totalZerado: totalZerado
            );

            return (data, dadosPaginated);
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