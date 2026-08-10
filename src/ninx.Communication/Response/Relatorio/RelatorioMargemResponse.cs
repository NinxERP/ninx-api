namespace ninx.Communication
{
    public class RelatorioMargemResponse
    {
        public List<ProdutoMargemResumo> Produtos { get; set; } = new();
        public List<CategoriaMargemResumo> Categorias { get; set; } = new();
        public int ProdutosSemPrecoCusto { get; set; }
    }

    public class ProdutoMargemResumo
    {
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = null!;
        public decimal QuantidadeVendida { get; set; }
        public decimal ReceitaTotal { get; set; }
        public decimal CustoTotal { get; set; }
        public decimal LucroTotal => ReceitaTotal - CustoTotal;
        public decimal MargemPercentual => ReceitaTotal > 0 ? Math.Round(LucroTotal / ReceitaTotal * 100, 2) : 0;
    }

    public class CategoriaMargemResumo
    {
        public int? CategoriaID { get; set; }
        public string CategoriaNome { get; set; } = null!;
        public decimal ReceitaTotal { get; set; }
        public decimal CustoTotal { get; set; }
        public decimal LucroTotal => ReceitaTotal - CustoTotal;
        public decimal MargemPercentual => ReceitaTotal > 0 ? Math.Round(LucroTotal / ReceitaTotal * 100, 2) : 0;
    }
}
