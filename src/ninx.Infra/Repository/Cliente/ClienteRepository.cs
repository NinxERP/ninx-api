using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository.ClienteRepository
{
    public class ClienteRepository : RepositoryBase<Cliente>, IClienteRepository
    {
        private readonly NinxDB _context;
        public ClienteRepository(NinxDB context) : base(context)
        {
            _context = context;
        }
    }
}
