using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class MovimentacaoEstoqueRepository : RepositoryBase<MovimentacaoEstoque>, IMovimentacaoEstoqueRepository
    {
        private readonly NinxDB _context;
        public MovimentacaoEstoqueRepository(NinxDB context) : base(context)
        {
            _context = context;
        }
    }
}
