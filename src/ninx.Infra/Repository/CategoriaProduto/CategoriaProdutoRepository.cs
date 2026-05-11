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
    }
}
