using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class AssinaturaEletronicaRepository : RepositoryBase<AssinaturaEletronica>, IAssinaturaEletronicaRepository
    {
        private readonly NinxDB _context;
        public AssinaturaEletronicaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }
    }
}
