using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;
using ninx.Infra.Repository.Base;

namespace ninx.Infra.Repository
{
    public class ProdutoRepository : RepositoryBase<Produto>, IProdutoRepository
    {
        private readonly AppDbContext _context;

        public ProdutoRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Produto>> GetAllByComercioAsync(int comercioId)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Include(x => x.Categoria)
                .Where(x => x.ComercioID == comercioId)
                .ToListAsync();
        }

        public async Task<Produto?> GetByCodigoBarrasAsync(int comercioId, string codigoBarras)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Include(x => x.Categoria)
                .FirstOrDefaultAsync(x => x.ComercioID == comercioId && x.CodigoBarras == codigoBarras);
        }

        public async Task<IEnumerable<Produto>> GetByNomeAsync(int comercioId, string nome)
        {
            return await _context.Produtos
                .AsNoTracking()
                .Include(x => x.Categoria)
                .Where(x => x.ComercioID == comercioId && x.Nome.Contains(nome))
                .ToListAsync();
        }
    }
}