using Microsoft.EntityFrameworkCore;
using ninx.Communication;
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

        public async Task<(IEnumerable<Cliente> Data, int TotalCount, int TotalAtivos)> GetClienteComercioByComercioId(int comercioId, PaginationRequest request)
        {
            var query = _context.Clientes
                .AsNoTracking()
                .Include(x => x.Comercio)
                .Where(x => x.ComercioID == comercioId);

            var totalCount = await query.CountAsync();

            var data = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var totalAtivos = await query.CountAsync(x => x.Ativo == true);
            return (data, totalCount, totalAtivos);
        }

    }
}
