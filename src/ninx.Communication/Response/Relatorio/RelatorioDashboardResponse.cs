namespace ninx.Communication
{
    public class RelatorioDashboardResponse
    {
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public FaturamentoResumo Faturamento { get; set; } = new();
        public List<FormaPagamentoResumo> FormasPagamento { get; set; } = new();
        public FiadoResumo Fiado { get; set; } = new();
        public List<ProdutoVendidoResumo> ProdutosMaisVendidos { get; set; } = new();
        public List<ProdutoVendidoResumo> ProdutosMenosVendidos { get; set; } = new();
        public List<EstoqueBaixoResumo> EstoqueBaixo { get; set; } = new();
        public CancelamentoResumo Cancelamentos { get; set; } = new();
    }

    public class FaturamentoResumo
    {
        public decimal Total { get; set; }
        public int QuantidadeVendas { get; set; }
        public decimal TicketMedio => QuantidadeVendas > 0 ? Total / QuantidadeVendas : 0;
    }

    public class FormaPagamentoResumo
    {
        public string FormaPagamento { get; set; } = null!;
        public decimal Valor { get; set; }
        public int Quantidade { get; set; }
    }

    public class FiadoResumo
    {
        public decimal SaldoDevedorTotal { get; set; }
        public int VendasEmAberto { get; set; }
    }

    public class ProdutoVendidoResumo
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = null!;
        public decimal QuantidadeVendida { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class EstoqueBaixoResumo
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = null!;
        public decimal QuantidadeAtual { get; set; }
        public decimal QuantidadeMinima { get; set; }
    }

    public class CancelamentoResumo
    {
        public int Quantidade { get; set; }
        public decimal ValorCancelado { get; set; }
    }
}
