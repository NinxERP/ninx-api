using System.Linq.Expressions;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Domain.Regras
{
    /// <summary>
    /// Regra única do saldo devedor do fiado. Antes, a mesma conta era repetida em
    /// serviços, repositórios e relatórios, e a listagem de clientes nem filtrava os
    /// pagamentos estornados. Toda leitura de saldo deve passar por aqui.
    /// </summary>
    /// <remarks>
    /// A regra tem duas representações lado a lado, de propósito: o EF Core só traduz
    /// para SQL árvores de expressão (<see cref="PagamentoEfetivo"/>,
    /// <see cref="VendaEmAberto"/>), e o código em memória usa os métodos compilados a
    /// partir delas. Como os métodos derivam das expressões, as duas formas não têm
    /// como divergir.
    /// </remarks>
    public static class SaldoDevedor
    {
        /// <summary>Pagamento que abate a dívida. Estornos e espelhos negativos ficam de fora.</summary>
        public static readonly Expression<Func<PagamentoVenda, bool>> PagamentoEfetivo =
            p => p.Status == StatusPagamento.Pago;

        /// <summary>Venda que compõe a dívida atual de um cliente.</summary>
        public static readonly Expression<Func<Venda, bool>> VendaEmAberto =
            v => v.TipoVenda == TipoVenda.Fiado && v.Status == StatusVenda.Aberta;

        private static readonly Func<PagamentoVenda, bool> EhPagamentoEfetivoCompilado = PagamentoEfetivo.Compile();
        private static readonly Func<Venda, bool> EhVendaEmAbertoCompilado = VendaEmAberto.Compile();

        public static bool EhPagamentoEfetivo(PagamentoVenda pagamento) => EhPagamentoEfetivoCompilado(pagamento);

        public static bool EhVendaEmAberto(Venda venda) => EhVendaEmAbertoCompilado(venda);

        public static decimal TotalPago(IEnumerable<PagamentoVenda> pagamentos) =>
            pagamentos.Where(EhPagamentoEfetivoCompilado).Sum(p => p.Valor);

        /// <summary>Saldo de uma venda: total menos os pagamentos efetivos.</summary>
        public static decimal DaVenda(Venda venda) => venda.Total - TotalPago(venda.PagamentosVenda);

        /// <summary>Saldo de um cliente: soma dos saldos das vendas em aberto.</summary>
        public static decimal DoCliente(IEnumerable<Venda> vendas) =>
            vendas.Where(EhVendaEmAbertoCompilado).Sum(DaVenda);
    }
}
