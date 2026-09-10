namespace ninx.Communication
{
    public class ProdutoCurvaAbcResponse
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = null!;
        public decimal QuantidadeVendida { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal PercentualReceita { get; set; }
        public decimal PercentualAcumulado { get; set; }
        public string Classificacao { get; set; } = null!;
    }
}
