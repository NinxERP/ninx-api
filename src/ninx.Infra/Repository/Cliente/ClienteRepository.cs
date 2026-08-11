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

        public async Task<Cliente?> GetByIdAndComercioIdAsync(int clienteId, int comercioId)
        {
            return await _context.Clientes
                .Include(x => x.Comercio)
                .FirstOrDefaultAsync(x => x.ClienteID == clienteId && x.ComercioID == comercioId);
        }

        public async Task<(IEnumerable<Cliente> Data, int TotalFiltrado, MetricsSummary Metrics)> GetClienteComercioByComercioId(
            int comercioId,
            PaginationRequest request)
        {
            var queryBase = _context.Clientes
                .AsNoTracking()
                .Where(x => x.ComercioID == comercioId);

            var metrics = await queryBase
                .GroupBy(_ => 1)
                .Select(g => new MetricsSummary
                {
                    TotalGeral = g.Count(),
                    TotalAtivos = g.Count(x => x.Ativo)
                })
                .FirstOrDefaultAsync() ?? new MetricsSummary();

            if (metrics.TotalGeral == 0)
            {
                return (Enumerable.Empty<Cliente>(), 0, metrics);
            }

            var queryData = queryBase;

            if (request.Status != null && request.Status.Any(s => !string.IsNullOrWhiteSpace(s)))
            {
                var ativo = request.Status.Any(s => string.Equals(s, "ativos", StringComparison.OrdinalIgnoreCase));
                queryData = queryData.Where(x => x.Ativo == ativo);
            }

            int totalFiltrado = await queryData.CountAsync();

            var data = await queryData
                .Include(x => x.Comercio)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return (data, totalFiltrado, metrics);
        }

    }
}
