using ninx.Communication;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class RelatorioService : IRelatorioService
    {
        private const int QuantidadeProdutosRanking = 5;
        private const int QuantidadeEstoqueBaixo = 20;

        private readonly IRelatorioRepository _relatorioRepository;
        private readonly IUsuarioComercioRepository _usuarioComercioRepository;

        public RelatorioService(IRelatorioRepository relatorioRepository, IUsuarioComercioRepository usuarioComercioRepository)
        {
            _relatorioRepository = relatorioRepository;
            _usuarioComercioRepository = usuarioComercioRepository;
        }

        public async Task<RelatorioDashboardResponse> GetDashboardAsync(int comercioId, RelatorioDashboardRequest request)
        {
            var (inicio, fim) = ResolverPeriodo(request);

            var faturamento = await _relatorioRepository.GetFaturamentoResumoAsync(comercioId, inicio, fim);
            var formasPagamento = await _relatorioRepository.GetFormasPagamentoResumoAsync(comercioId, inicio, fim);
            var fiado = await _relatorioRepository.GetFiadoResumoAsync(comercioId);
            var produtosMaisVendidos = await _relatorioRepository.GetProdutosMaisVendidosAsync(comercioId, inicio, fim, QuantidadeProdutosRanking);
            var produtosMenosVendidos = await _relatorioRepository.GetProdutosMenosVendidosAsync(comercioId, inicio, fim, QuantidadeProdutosRanking);
            var estoqueBaixo = await _relatorioRepository.GetEstoqueBaixoAsync(comercioId, QuantidadeEstoqueBaixo);
            var cancelamentos = await _relatorioRepository.GetCancelamentosResumoAsync(comercioId, inicio, fim);

            return new RelatorioDashboardResponse
            {
                Inicio = inicio,
                Fim = fim,
                Faturamento = faturamento,
                FormasPagamento = formasPagamento,
                Fiado = fiado,
                ProdutosMaisVendidos = produtosMaisVendidos,
                ProdutosMenosVendidos = produtosMenosVendidos,
                EstoqueBaixo = estoqueBaixo,
                Cancelamentos = cancelamentos
            };
        }

        public async Task<List<ProdutoCurvaAbcResponse>> GetCurvaAbcAsync(int comercioId, RelatorioDashboardRequest request)
        {
            var (inicio, fim) = ResolverPeriodo(request);
            var produtos = await _relatorioRepository.GetProdutosVendidosAsync(comercioId, inicio, fim);

            var totalGeral = produtos.Sum(p => p.ValorTotal);
            if (totalGeral <= 0)
                return new List<ProdutoCurvaAbcResponse>();

            var resultado = new List<ProdutoCurvaAbcResponse>();
            decimal acumulado = 0;

            foreach (var produto in produtos)
            {
                var percentual = produto.ValorTotal / totalGeral * 100;
                acumulado += percentual;

                resultado.Add(new ProdutoCurvaAbcResponse
                {
                    ProdutoID = produto.ProdutoID,
                    ProdutoNome = produto.ProdutoNome,
                    QuantidadeVendida = produto.QuantidadeVendida,
                    ValorTotal = produto.ValorTotal,
                    PercentualReceita = Math.Round(percentual, 2),
                    PercentualAcumulado = Math.Round(acumulado, 2),
                    Classificacao = acumulado <= 80 ? "A" : acumulado <= 95 ? "B" : "C"
                });
            }

            return resultado;
        }

        public async Task<RelatorioMargemResponse> GetMargemAsync(int comercioId, RelatorioDashboardRequest request)
        {
            var (inicio, fim) = ResolverPeriodo(request);

            var produtos = await _relatorioRepository.GetMargemPorProdutoAsync(comercioId, inicio, fim);
            var categorias = await _relatorioRepository.GetMargemPorCategoriaAsync(comercioId, inicio, fim);
            var produtosSemCusto = await _relatorioRepository.GetQuantidadeProdutosVendidosSemCustoAsync(comercioId, inicio, fim);

            return new RelatorioMargemResponse
            {
                Produtos = produtos,
                Categorias = categorias,
                ProdutosSemPrecoCusto = produtosSemCusto
            };
        }

        public async Task<RelatorioAgingResponse> GetAgingRecebiveisAsync(int comercioId)
        {
            var vendas = await _relatorioRepository.GetVendasFiadoEmAbertoAsync(comercioId);

            var response = new RelatorioAgingResponse { Vendas = vendas };

            foreach (var venda in vendas)
            {
                var bucket = venda.DiasEmAberto <= 30 ? response.Ate30Dias
                    : venda.DiasEmAberto <= 60 ? response.De31a60Dias
                    : response.Acima60Dias;

                bucket.Valor += venda.SaldoDevedor;
                bucket.Quantidade++;
            }

            return response;
        }

        public async Task<List<VendedorDesempenhoResumo>> GetDesempenhoVendedoresAsync(int comercioId, RelatorioDashboardRequest request)
        {
            var (inicio, fim) = ResolverPeriodo(request);
            return await _relatorioRepository.GetDesempenhoVendedoresAsync(comercioId, inicio, fim);
        }

        public async Task<RelatorioPicoVendasResponse> GetPicoVendasAsync(int comercioId, RelatorioDashboardRequest request)
        {
            var (inicio, fim) = ResolverPeriodo(request);

            var porHora = await _relatorioRepository.GetPicoPorHoraAsync(comercioId, inicio, fim);
            var porDiaSemana = await _relatorioRepository.GetPicoPorDiaSemanaAsync(comercioId, inicio, fim);

            return new RelatorioPicoVendasResponse
            {
                PorHora = porHora,
                PorDiaSemana = porDiaSemana
            };
        }

        public async Task<List<ClienteInativoResumo>> GetClientesInativosAsync(int comercioId, RelatorioClientesInativosRequest request)
        {
            if (request.DiasSemComprar <= 0)
                throw new BadRequestException("A quantidade de dias sem comprar deve ser maior que zero.");

            return await _relatorioRepository.GetClientesInativosAsync(comercioId, request.DiasSemComprar);
        }

        public async Task<List<ClienteLimiteCreditoResumo>> GetUsoLimiteCreditoAsync(int comercioId)
        {
            return await _relatorioRepository.GetUsoLimiteCreditoAsync(comercioId);
        }

        public async Task<RelatorioGiroEstoqueResponse> GetGiroEstoqueAsync(int comercioId, RelatorioDashboardRequest request)
        {
            var (inicio, fim) = ResolverPeriodo(request);

            var produtos = await _relatorioRepository.GetGiroEstoqueAsync(comercioId, inicio, fim);
            var produtosParados = await _relatorioRepository.GetProdutosParadosAsync(comercioId, inicio, fim);

            return new RelatorioGiroEstoqueResponse
            {
                Produtos = produtos,
                ProdutosParados = produtosParados
            };
        }

        public async Task<List<ProdutoVencendoResumo>> GetProdutosVencendoAsync(int comercioId, RelatorioProdutosVencendoRequest request)
        {
            if (request.DiasLimite <= 0)
                throw new BadRequestException("A quantidade de dias limite deve ser maior que zero.");

            return await _relatorioRepository.GetProdutosVencendoAsync(comercioId, request.DiasLimite);
        }

        public async Task<RelatorioComparativoComerciosResponse> GetComparativoComerciosAsync(int usuarioId, RelatorioDashboardRequest request)
        {
            var (inicio, fim) = ResolverPeriodo(request);

            var vinculos = await _usuarioComercioRepository.GetByUsuarioIdAsync(usuarioId);
            var comercioIds = vinculos.Where(v => v.Ativo).Select(v => v.ComercioID).Distinct().ToList();

            if (!comercioIds.Any())
                throw new BadRequestException("Usuário não possui comércios vinculados.");

            var comparativo = await _relatorioRepository.GetComparativoComerciosAsync(comercioIds, inicio, fim);

            return new RelatorioComparativoComerciosResponse { Comercios = comparativo };
        }

        private static (DateTime Inicio, DateTime Fim) ResolverPeriodo(RelatorioDashboardRequest request)
        {
            var agora = DateTime.UtcNow;
            var inicio = request.Inicio ?? new DateTime(agora.Year, agora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var fim = request.Fim.HasValue
                ? request.Fim.Value.Date.AddDays(1).AddTicks(-1)
                : agora;

            if (inicio > fim)
                throw new BadRequestException("A data de início não pode ser maior que a data de fim.");

            return (inicio, fim);
        }
    }
}
