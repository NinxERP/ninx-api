using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class ItemVendaRepository : RepositoryBase<ItemVenda>, IItemVendaRepository
    {
        private readonly NinxDB _context;
        public ItemVendaRepository(NinxDB context) : base(context)
        {
            _context = context;

        }
    }
}
