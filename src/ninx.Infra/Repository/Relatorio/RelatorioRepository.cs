using Microsoft.EntityFrameworkCore;
using ninx.Communication;
using ninx.Data.Context;
using ninx.Domain.Enums;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class RelatorioRepository : IRelatorioRepository
    {
        private readonly NinxDB _context;

        public RelatorioRepository(NinxDB context)
        {
            _context = context;
        }

        public async Task<FaturamentoResumo> GetFaturamentoResumoAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            var resumo = await _context.Vendas
                .AsNoTracking()
                .Where(v => v.ComercioID == comercioId
                    && v.Status == StatusVenda.Finalizada
                    && v.CriadoEm >= inicio && v.CriadoEm <= fim)
                .GroupBy(_ => 1)
                .Select(g => new FaturamentoResumo
                {
                    Total = g.Sum(v => v.Total),
                    QuantidadeVendas = g.Count()
                })
                .FirstOrDefaultAsync();

            return resumo ?? new FaturamentoResumo();
        }

        public async Task<List<FormaPagamentoResumo>> GetFormasPagamentoResumoAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            return await _context.PagamentoVendas
                .AsNoTracking()
                .Where(p => p.Venda.ComercioID == comercioId
                    && p.Status == StatusPagamento.Pago
                    && p.CriadoEm >= inicio && p.CriadoEm <= fim)
                .GroupBy(p => p.FormaPagamento)
                .Select(g => new FormaPagamentoResumo
                {
                    FormaPagamento = g.Key.ToString(),
                    Valor = g.Sum(p => p.Valor),
                    Quantidade = g.Count()
                })
                .OrderByDescending(x => x.Valor)
                .ToListAsync();
        }

        public async Task<FiadoResumo> GetFiadoResumoAsync(int comercioId)
        {
            var vendasEmAberto = await _context.Vendas
                .AsNoTracking()
                .Where(v => v.ComercioID == comercioId
                    && v.TipoVenda == TipoVenda.Fiado
                    && v.Status == StatusVenda.Finalizada)
                .Select(v => new
                {
                    v.Total,
                    TotalPago = v.PagamentosVenda
                        .Where(p => p.Status == StatusPagamento.Pago)
                        .Sum(p => (decimal?)p.Valor) ?? 0
                })
                .Where(x => x.Total > x.TotalPago)
                .ToListAsync();

            return new FiadoResumo
            {
                VendasEmAberto = vendasEmAberto.Count,
                SaldoDevedorTotal = vendasEmAberto.Sum(x => x.Total - x.TotalPago)
            };
        }

        public async Task<List<ProdutoVendidoResumo>> GetProdutosMaisVendidosAsync(int comercioId, DateTime inicio, DateTime fim, int quantidade)
        {
            return await GetProdutosVendidosQuery(comercioId, inicio, fim)
                .OrderByDescending(x => x.ValorTotal)
                .Take(quantidade)
                .ToListAsync();
        }

        public async Task<List<ProdutoVendidoResumo>> GetProdutosMenosVendidosAsync(int comercioId, DateTime inicio, DateTime fim, int quantidade)
        {
            return await GetProdutosVendidosQuery(comercioId, inicio, fim)
                .OrderBy(x => x.ValorTotal)
                .Take(quantidade)
                .ToListAsync();
        }

        private IQueryable<ProdutoVendidoResumo> GetProdutosVendidosQuery(int comercioId, DateTime inicio, DateTime fim)
        {
            return _context.ItemVendas
                .AsNoTracking()
                .Where(i => i.Venda.ComercioID == comercioId
                    && i.Venda.Status == StatusVenda.Finalizada
                    && i.Venda.CriadoEm >= inicio && i.Venda.CriadoEm <= fim)
                .GroupBy(i => new { i.ProdutoID, i.ProdutoNome })
                .Select(g => new ProdutoVendidoResumo
                {
                    ProdutoID = g.Key.ProdutoID,
                    ProdutoNome = g.Key.ProdutoNome,
                    QuantidadeVendida = g.Sum(i => i.Quantidade),
                    ValorTotal = g.Sum(i => i.Subtotal)
                });
        }

        public async Task<List<EstoqueBaixoResumo>> GetEstoqueBaixoAsync(int comercioId, int quantidade)
        {
            return await _context.Estoques
                .AsNoTracking()
                .Where(e => e.ComercioID == comercioId
                    && e.Produto.Ativo
                    && e.Quantidade < e.QuantidadeMinima)
                .OrderBy(e => e.Quantidade)
                .Select(e => new EstoqueBaixoResumo
                {
                    ProdutoID = e.ProdutoID,
                    ProdutoNome = e.Produto.Nome,
                    QuantidadeAtual = e.Quantidade,
                    QuantidadeMinima = e.QuantidadeMinima
                })
                .Take(quantidade)
                .ToListAsync();
        }

        public async Task<CancelamentoResumo> GetCancelamentosResumoAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            var resumo = await _context.Vendas
                .AsNoTracking()
                .Where(v => v.ComercioID == comercioId
                    && (v.Status == StatusVenda.Cancelada || v.Status == StatusVenda.Estornada)
                    && v.CriadoEm >= inicio && v.CriadoEm <= fim)
                .GroupBy(_ => 1)
                .Select(g => new CancelamentoResumo
                {
                    Quantidade = g.Count(),
                    ValorCancelado = g.Sum(v => v.Total)
                })
                .FirstOrDefaultAsync();

            return resumo ?? new CancelamentoResumo();
        }

        public async Task<List<ProdutoVendidoResumo>> GetProdutosVendidosAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            return await GetProdutosVendidosQuery(comercioId, inicio, fim)
                .OrderByDescending(x => x.ValorTotal)
                .ToListAsync();
        }

        public async Task<List<ProdutoMargemResumo>> GetMargemPorProdutoAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            return await _context.ItemVendas
                .AsNoTracking()
                .Where(i => i.Venda.ComercioID == comercioId
                    && i.Venda.Status == StatusVenda.Finalizada
                    && i.Venda.CriadoEm >= inicio && i.Venda.CriadoEm <= fim
                    && i.Produto.PrecoCusto != null)
                .GroupBy(i => new { i.ProdutoID, i.ProdutoNome })
                .Select(g => new ProdutoMargemResumo
                {
                    ProdutoID = g.Key.ProdutoID,
                    ProdutoNome = g.Key.ProdutoNome,
                    QuantidadeVendida = g.Sum(i => i.Quantidade),
                    ReceitaTotal = g.Sum(i => i.Subtotal),
                    CustoTotal = g.Sum(i => i.Quantidade * i.Produto.PrecoCusto!.Value)
                })
                .OrderByDescending(x => x.ReceitaTotal)
                .ToListAsync();
        }

        public async Task<List<CategoriaMargemResumo>> GetMargemPorCategoriaAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            return await _context.ItemVendas
                .AsNoTracking()
                .Where(i => i.Venda.ComercioID == comercioId
                    && i.Venda.Status == StatusVenda.Finalizada
                    && i.Venda.CriadoEm >= inicio && i.Venda.CriadoEm <= fim
                    && i.Produto.PrecoCusto != null)
                .GroupBy(i => i.Produto.CategoriaID)
                .Select(g => new CategoriaMargemResumo
                {
                    CategoriaID = g.Key,
                    CategoriaNome = g.Max(i => i.Produto.Categoria != null ? i.Produto.Categoria.Nome : "Sem categoria"),
                    ReceitaTotal = g.Sum(i => i.Subtotal),
                    CustoTotal = g.Sum(i => i.Quantidade * i.Produto.PrecoCusto!.Value)
                })
                .OrderByDescending(x => x.ReceitaTotal)
                .ToListAsync();
        }

        public async Task<int> GetQuantidadeProdutosVendidosSemCustoAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            return await _context.ItemVendas
                .AsNoTracking()
                .Where(i => i.Venda.ComercioID == comercioId
                    && i.Venda.Status == StatusVenda.Finalizada
                    && i.Venda.CriadoEm >= inicio && i.Venda.CriadoEm <= fim
                    && i.Produto.PrecoCusto == null)
                .Select(i => i.ProdutoID)
                .Distinct()
                .CountAsync();
        }

        public async Task<List<VendaEmAbertoResumo>> GetVendasFiadoEmAbertoAsync(int comercioId)
        {
            var agora = DateTime.UtcNow;

            return await _context.Vendas
                .AsNoTracking()
                .Where(v => v.ComercioID == comercioId
                    && v.TipoVenda == TipoVenda.Fiado
                    && v.Status == StatusVenda.Finalizada)
                .Select(v => new VendaEmAbertoResumo
                {
                    VendaID = v.VendaID,
                    ClienteID = v.ClienteID,
                    ClienteNome = v.Cliente != null ? v.Cliente.Nome : "Cliente não identificado",
                    SaldoDevedor = v.Total - (v.PagamentosVenda.Where(p => p.Status == StatusPagamento.Pago).Sum(p => (decimal?)p.Valor) ?? 0),
                    CriadoEm = v.CriadoEm,
                    DiasEmAberto = EF.Functions.DateDiffDay(v.CriadoEm, agora)
                })
                .Where(x => x.SaldoDevedor > 0)
                .OrderByDescending(x => x.DiasEmAberto)
                .ToListAsync();
        }

        public async Task<List<VendedorDesempenhoResumo>> GetDesempenhoVendedoresAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            return await _context.Vendas
                .AsNoTracking()
                .Where(v => v.ComercioID == comercioId
                    && v.Status == StatusVenda.Finalizada
                    && v.CriadoEm >= inicio && v.CriadoEm <= fim)
                .GroupBy(v => new { v.UsuarioID, v.Usuario.Nome })
                .Select(g => new VendedorDesempenhoResumo
                {
                    UsuarioID = g.Key.UsuarioID,
                    UsuarioNome = g.Key.Nome,
                    QuantidadeVendas = g.Count(),
                    ValorTotal = g.Sum(v => v.Total)
                })
                .OrderByDescending(x => x.ValorTotal)
                .ToListAsync();
        }

        // Vendas.CriadoEm é gravado em UTC. Não há fuso por comércio persistido no banco;
        // assume-se o fuso padrão da operação (America/Sao_Paulo, UTC-3, sem horário de verão)
        // para que "hora de pico"/"dia da semana" reflitam o horário local da loja.
        private static readonly TimeSpan FusoHorarioPadrao = TimeSpan.FromHours(-3);

        public async Task<List<PicoPorHoraResumo>> GetPicoPorHoraAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            var vendas = await _context.Vendas
                .AsNoTracking()
                .Where(v => v.ComercioID == comercioId
                    && v.Status == StatusVenda.Finalizada
                    && v.CriadoEm >= inicio && v.CriadoEm <= fim)
                .Select(v => new { v.CriadoEm, v.Total })
                .ToListAsync();

            return vendas
                .GroupBy(v => (v.CriadoEm + FusoHorarioPadrao).Hour)
                .Select(g => new PicoPorHoraResumo
                {
                    Hora = g.Key,
                    QuantidadeVendas = g.Count(),
                    ValorTotal = g.Sum(v => v.Total)
                })
                .OrderBy(x => x.Hora)
                .ToList();
        }

        public async Task<List<PicoPorDiaSemanaResumo>> GetPicoPorDiaSemanaAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            var vendas = await _context.Vendas
                .AsNoTracking()
                .Where(v => v.ComercioID == comercioId
                    && v.Status == StatusVenda.Finalizada
                    && v.CriadoEm >= inicio && v.CriadoEm <= fim)
                .Select(v => new { v.CriadoEm, v.Total })
                .ToListAsync();

            return vendas
                .GroupBy(v => (v.CriadoEm + FusoHorarioPadrao).DayOfWeek)
                .Select(g => new
                {
                    DiaSemana = g.Key,
                    QuantidadeVendas = g.Count(),
                    ValorTotal = g.Sum(v => v.Total)
                })
                .OrderBy(x => x.DiaSemana)
                .Select(x => new PicoPorDiaSemanaResumo
                {
                    DiaSemana = x.DiaSemana.ToString(),
                    QuantidadeVendas = x.QuantidadeVendas,
                    ValorTotal = x.ValorTotal
                })
                .ToList();
        }

        public async Task<List<ClienteInativoResumo>> GetClientesInativosAsync(int comercioId, int diasSemComprar)
        {
            var agora = DateTime.UtcNow;
            var referencia = agora.AddDays(-diasSemComprar);

            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c => c.ComercioID == comercioId && c.Ativo)
                .Select(c => new ClienteInativoResumo
                {
                    ClienteID = c.ClienteID,
                    ClienteNome = c.Nome,
                    Telefone = c.Telefone,
                    UltimaCompra = _context.Vendas
                        .Where(v => v.ClienteID == c.ClienteID && v.Status == StatusVenda.Finalizada)
                        .Max(v => (DateTime?)v.CriadoEm)
                })
                .Where(x => x.UltimaCompra == null || x.UltimaCompra < referencia)
                .OrderBy(x => x.UltimaCompra)
                .ToListAsync();

            foreach (var cliente in clientes)
            {
                cliente.DiasSemComprar = cliente.UltimaCompra.HasValue
                    ? (int)(agora - cliente.UltimaCompra.Value).TotalDays
                    : null;
            }

            return clientes;
        }

        public async Task<List<ClienteLimiteCreditoResumo>> GetUsoLimiteCreditoAsync(int comercioId)
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
                .Where(c => c.ComercioID == comercioId && c.Ativo && c.LimiteCredito != null && c.LimiteCredito > 0)
                .Select(c => new ClienteLimiteCreditoResumo
                {
                    ClienteID = c.ClienteID,
                    ClienteNome = c.Nome,
                    LimiteCredito = c.LimiteCredito!.Value,
                    SaldoDevedor = _context.Vendas
                        .Where(v => v.ClienteID == c.ClienteID
                            && v.TipoVenda == TipoVenda.Fiado
                            && v.Status == StatusVenda.Finalizada)
                        .Sum(v => v.Total - (v.PagamentosVenda.Where(p => p.Status == StatusPagamento.Pago).Sum(p => (decimal?)p.Valor) ?? 0))
                })
                .Where(x => x.SaldoDevedor > 0)
                .ToListAsync();

            return clientes.OrderByDescending(x => x.PercentualUtilizado).ToList();
        }

        public async Task<List<GiroEstoqueResumo>> GetGiroEstoqueAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            return await _context.ItemVendas
                .AsNoTracking()
                .Where(i => i.Venda.ComercioID == comercioId
                    && i.Venda.Status == StatusVenda.Finalizada
                    && i.Venda.CriadoEm >= inicio && i.Venda.CriadoEm <= fim)
                .GroupBy(i => new { i.ProdutoID, i.ProdutoNome })
                .Select(g => new GiroEstoqueResumo
                {
                    ProdutoID = g.Key.ProdutoID,
                    ProdutoNome = g.Key.ProdutoNome,
                    QuantidadeVendida = g.Sum(i => i.Quantidade),
                    EstoqueAtual = _context.Estoques
                        .Where(e => e.ProdutoID == g.Key.ProdutoID && e.ComercioID == comercioId)
                        .Select(e => e.Quantidade)
                        .FirstOrDefault()
                })
                .OrderByDescending(x => x.QuantidadeVendida)
                .ToListAsync();
        }

        public async Task<List<ProdutoParadoResumo>> GetProdutosParadosAsync(int comercioId, DateTime inicio, DateTime fim)
        {
            return await _context.Estoques
                .AsNoTracking()
                .Where(e => e.ComercioID == comercioId
                    && e.Produto.Ativo
                    && e.Quantidade > 0
                    && !_context.ItemVendas.Any(i => i.ProdutoID == e.ProdutoID
                        && i.Venda.ComercioID == comercioId
                        && i.Venda.Status == StatusVenda.Finalizada
                        && i.Venda.CriadoEm >= inicio && i.Venda.CriadoEm <= fim))
                .Select(e => new ProdutoParadoResumo
                {
                    ProdutoID = e.ProdutoID,
                    ProdutoNome = e.Produto.Nome,
                    EstoqueAtual = e.Quantidade
                })
                .OrderByDescending(x => x.EstoqueAtual)
                .ToListAsync();
        }

        public async Task<List<ProdutoVencendoResumo>> GetProdutosVencendoAsync(int comercioId, int diasLimite)
        {
            var agora = DateTime.UtcNow;
            var limite = agora.AddDays(diasLimite);

            var produtos = await _context.Produtos
                .AsNoTracking()
                .Where(p => p.ComercioID == comercioId
                    && p.Ativo
                    && p.Validade != null
                    && p.Validade <= limite
                    && p.Estoque != null && p.Estoque.Quantidade > 0)
                .Select(p => new ProdutoVencendoResumo
                {
                    ProdutoID = p.ProdutoID,
                    ProdutoNome = p.Nome,
                    Validade = p.Validade!.Value,
                    QuantidadeEmEstoque = p.Estoque!.Quantidade
                })
                .OrderBy(x => x.Validade)
                .ToListAsync();

            foreach (var produto in produtos)
            {
                produto.DiasParaVencer = (int)(produto.Validade.Date - agora.Date).TotalDays;
            }

            return produtos;
        }

        public async Task<List<ComercioComparativoResumo>> GetComparativoComerciosAsync(List<int> comercioIds, DateTime inicio, DateTime fim)
        {
            return await _context.Vendas
                .AsNoTracking()
                .Where(v => comercioIds.Contains(v.ComercioID)
                    && v.Status == StatusVenda.Finalizada
                    && v.CriadoEm >= inicio && v.CriadoEm <= fim)
                .GroupBy(v => new { v.ComercioID, v.Comercio.NomeComercio })
                .Select(g => new ComercioComparativoResumo
                {
                    ComercioID = g.Key.ComercioID,
                    ComercioNome = g.Key.NomeComercio,
                    FaturamentoTotal = g.Sum(v => v.Total),
                    QuantidadeVendas = g.Count()
                })
                .OrderByDescending(x => x.FaturamentoTotal)
                .ToListAsync();
        }
    }
}
