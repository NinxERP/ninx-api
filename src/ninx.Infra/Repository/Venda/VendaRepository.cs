using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class VendaRepository : RepositoryBase<Venda>, IVendaRepository
    {
        private readonly NinxDB _context;

        public VendaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

    }
}
