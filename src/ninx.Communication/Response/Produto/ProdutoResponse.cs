namespace ninx.Communication.Response.Produto
{
    public class ProdutoResponse
    {
        public int ProdutoID { get; set; }
        public int ComercioID { get; set; }
        public int? CategoriaID { get; set; }
        public string? CategoriaNome { get; set; }
        public string Nome { get; set; } = null!;
        public string? CodigoBarras { get; set; }
        public decimal PrecoVenda { get; set; }
        public decimal? PrecoCusto { get; set; }
        public string UnidadeMedida { get; set; } = null!;
        public DateTime? Validade { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}