namespace ninx.Communication
{
    public class RelatorioGiroEstoqueResponse
    {
        public List<GiroEstoqueResumo> Produtos { get; set; } = new();
        public List<ProdutoParadoResumo> ProdutosParados { get; set; } = new();
    }

    public class GiroEstoqueResumo
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = null!;
        public decimal QuantidadeVendida { get; set; }
        public decimal EstoqueAtual { get; set; }
        public decimal Giro => EstoqueAtual > 0 ? Math.Round(QuantidadeVendida / EstoqueAtual, 2) : 0;
    }

    public class ProdutoParadoResumo
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = null!;
        public decimal EstoqueAtual { get; set; }
        public DateTime? UltimaVenda { get; set; }
        public int? DiasSemVender => UltimaVenda.HasValue ? (DateTime.UtcNow.Date - UltimaVenda.Value.Date).Days : null;
    }
}
