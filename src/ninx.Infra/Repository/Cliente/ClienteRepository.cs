using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository.ClienteRepository
{
    public class ClienteRepository : RepositoryBase<Cliente>, IClienteRepository
    {
        private readonly NinxDB _context;
        public ClienteRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Cliente>> GetByNomeAsync(string nome, int comercioId)
        {
            return await _context.Clientes
                .Where(x => x.Nome.Contains(nome) && x.ComercioID == comercioId)
                .ToListAsync();
        }
    }
}
