using FluentAssertions;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Regras;
using Xunit;

namespace ninx.Tests.Regras
{
    /// <summary>
    /// Regra única do saldo devedor. Todo cálculo de saldo do sistema — serviços,
    /// repositórios e relatórios — passa por <see cref="SaldoDevedor"/>.
    /// </summary>
    public class SaldoDevedorTests
    {
        private static Venda VendaFiado(decimal total, StatusVenda status = StatusVenda.Aberta, params PagamentoVenda[] pagamentos) => new()
        {
            VendaID = 1,
            Total = total,
            TipoVenda = TipoVenda.Fiado,
            Status = status,
            PagamentosVenda = pagamentos.ToList()
        };

        private static PagamentoVenda Pago(decimal valor) => new() { Valor = valor, Status = StatusPagamento.Pago };
        private static PagamentoVenda Estornado(decimal valor) => new() { Valor = valor, Status = StatusPagamento.Estornado };

        [Fact]
        public void DaVenda_SemPagamentos_SaldoEhOTotal()
        {
            SaldoDevedor.DaVenda(VendaFiado(100m)).Should().Be(100m);
        }

        [Fact]
        public void DaVenda_DescontaSomentePagamentosEfetivos()
        {
            var venda = VendaFiado(100m, StatusVenda.Aberta, Pago(30m), Pago(20m));
            SaldoDevedor.DaVenda(venda).Should().Be(50m);
        }

        [Fact]
        public void DaVenda_PagamentoEstornadoEEspelhoNegativo_NaoAbatem()
        {
            // O estorno marca o original como Estornado e insere um espelho negativo também
            // Estornado. Nenhum dos dois pode mexer no saldo.
            var venda = VendaFiado(100m, StatusVenda.Aberta, Pago(10m), Estornado(40m), Estornado(-40m));
            SaldoDevedor.DaVenda(venda).Should().Be(90m);
        }

        [Fact]
        public void TotalPago_IgnoraEstornados()
        {
            SaldoDevedor.TotalPago(new[] { Pago(10m), Estornado(99m), Pago(5m) }).Should().Be(15m);
        }

        [Fact]
        public void DoCliente_SomaApenasVendasFiadoEmAberto()
        {
            var vendas = new[]
            {
                VendaFiado(100m, StatusVenda.Aberta, Pago(40m)),     // 60
                VendaFiado(50m, StatusVenda.Aberta),                 // 50
                VendaFiado(80m, StatusVenda.Aguardando),             // ainda não assinada: fora
                VendaFiado(70m, StatusVenda.Estornada),              // estornada: fora
                new Venda { Total = 200m, TipoVenda = TipoVenda.Normal, Status = StatusVenda.Aberta } // não é fiado: fora
            };

            SaldoDevedor.DoCliente(vendas).Should().Be(110m);
        }

        [Theory]
        [InlineData(StatusPagamento.Pago, true)]
        [InlineData(StatusPagamento.Estornado, false)]
        public void PagamentoEfetivo_ExpressaoEMetodoConcordam(StatusPagamento status, bool esperado)
        {
            var pagamento = new PagamentoVenda { Status = status, Valor = 1m };

            SaldoDevedor.PagamentoEfetivo.Compile()(pagamento).Should().Be(esperado);
            SaldoDevedor.EhPagamentoEfetivo(pagamento).Should().Be(esperado);
        }

        [Theory]
        [InlineData(TipoVenda.Fiado, StatusVenda.Aberta, true)]
        [InlineData(TipoVenda.Fiado, StatusVenda.Aguardando, false)]
        [InlineData(TipoVenda.Fiado, StatusVenda.Estornada, false)]
        [InlineData(TipoVenda.Normal, StatusVenda.Aberta, false)]
        public void VendaEmAberto_ExpressaoEMetodoConcordam(TipoVenda tipo, StatusVenda status, bool esperado)
        {
            var venda = new Venda { TipoVenda = tipo, Status = status };

            SaldoDevedor.VendaEmAberto.Compile()(venda).Should().Be(esperado);
            SaldoDevedor.EhVendaEmAberto(venda).Should().Be(esperado);
        }
    }
}
