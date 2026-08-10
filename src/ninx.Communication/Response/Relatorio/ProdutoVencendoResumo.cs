namespace ninx.Communication
{
    public class ProdutoVencendoResumo
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = null!;
        public DateTime Validade { get; set; }
        public int DiasParaVencer { get; set; }
        public decimal QuantidadeEmEstoque { get; set; }
        public bool Vencido => DiasParaVencer < 0;
    }
}
