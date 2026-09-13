using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Interfaces;
using ninx.Domain.Regras;

namespace ninx.Infra.Repository
{
    public class VendaRepository : RepositoryBase<Venda>, IVendaRepository
    {
        private readonly NinxDB _context;

        public VendaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venda>> GetVendasFiltroAsync(DateTime? inicio, DateTime? fim, int comercioID, int? usuarioID)
        {
            var query = _context.Vendas
                .AsNoTracking()
                .Include(v => v.PagamentosVenda)
                .Where(v => v.ComercioID == comercioID);

            if (inicio.HasValue)
                query = query.Where(v => v.CriadoEm >= inicio.Value);

            if (fim.HasValue)
                query = query.Where(v => v.CriadoEm <= fim.Value);

            if (usuarioID.HasValue)
                query = query.Where(v => v.UsuarioID == usuarioID.Value);

            return await query.OrderByDescending(v => v.CriadoEm).ToListAsync();
        }

        public async Task<IEnumerable<Venda>> GetVendasByUsuarioIdAsync(int usuarioId, int comercioId)
        {
            return await _context.Vendas
                .AsNoTracking()
                .Include(v => v.ItensVenda)
                .Include(v => v.PagamentosVenda)
                .Where(v => v.UsuarioID == usuarioId && v.ComercioID == comercioId)
                .OrderByDescending(v => v.CriadoEm)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venda>> GetVendasByClienteIdAsync(int clienteId, int comercioId)
        {
            return await _context.Vendas
                .AsNoTracking()
                .Include(v => v.PagamentosVenda)
                .Where(v => v.ClienteID == clienteId && v.ComercioID == comercioId && v.Status == StatusVenda.Aberta)
                .OrderByDescending(v => v.CriadoEm)
                .ToListAsync();
        }

        public async Task<Venda?> GetByIdAsync(int id)
        {
            return await _context.Vendas
                .Include(v => v.ItensVenda)        
                .Include(v => v.PagamentosVenda)    
                .FirstOrDefaultAsync(v => v.VendaID == id);
        }

        public async Task<Venda?> GetByIdParaEstornoAsync(int id)
        {
            return await _context.Vendas
                .Include(v => v.ItensVenda)
                .Include(v => v.PagamentosVenda)
                .FirstOrDefaultAsync(v => v.VendaID == id);
        }

        public async Task<Venda?> GetByIdParaPagamentoFiadoAsync(int id)
        {
            return await _context.Vendas
                .Include(v => v.PagamentosVenda)
                .FirstOrDefaultAsync(v => v.VendaID == id);
        }

        public async Task<Venda?> GetByIdParaDetalheAsync(int id)
        {
            return await _context.Vendas
                .AsNoTracking()
                .Include(v => v.ItensVenda)
                .Include(v => v.PagamentosVenda)
                .FirstOrDefaultAsync(v => v.VendaID == id);
        }
        public async Task<Dictionary<int, decimal>> GetSaldoDevedorClientesPorComercio(int comercioId)
        {
            // Antes, esta soma não filtrava o status do pagamento: pagamentos estornados
            // entravam na conta. Agora usa o mesmo critério do restante do sistema.
            return await _context.Vendas
                .Where(SaldoDevedor.VendaEmAberto)
                .Where(v => v.ComercioID == comercioId && v.ClienteID != null)
                .GroupBy(v => v.ClienteID!.Value)
                .Select(g => new
                {
                    ClienteID = g.Key,
                    Saldo = g.Sum(v => v.Total)
                        - g.Sum(v => v.PagamentosVenda.AsQueryable().Where(SaldoDevedor.PagamentoEfetivo).Sum(p => (decimal?)p.Valor) ?? 0)
                })
                .ToDictionaryAsync(x => x.ClienteID, x => x.Saldo);
        }

        public async Task<IEnumerable<Venda>> GetVendasFiadoByClienteIDAsync(int? clienteId)
        {
            return await _context.Vendas
                .AsNoTracking()
                .Where(v => v.ClienteID == clienteId && v.TipoVenda == TipoVenda.Fiado && v.Status != StatusVenda.Cancelada && v.Status != StatusVenda.Estornada)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venda>> GetVendasFiadoAtivasPorClienteAsync(int clienteId)
        {
            // O "?? 0" evita que uma venda sem nenhum pagamento efetivo seja descartada:
            // sem ele, Total > NULL é falso no SQL e a venda sumiria da quitação global.
            return await _context.Vendas
                .Include(v => v.PagamentosVenda)
                .Where(SaldoDevedor.VendaEmAberto)
                .Where(v => v.ClienteID == clienteId)
                .Where(v => v.Total > (v.PagamentosVenda.AsQueryable()
                    .Where(SaldoDevedor.PagamentoEfetivo)
                    .Sum(p => (decimal?)p.Valor) ?? 0))
                .OrderBy(v => v.CriadoEm)
                .ToListAsync();
        }
    }
}

