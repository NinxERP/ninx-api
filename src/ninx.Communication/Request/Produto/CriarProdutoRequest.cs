namespace ninx.Communication.Request
{
    public class CriarProdutoRequest
    {
        public int ComercioID { get; set; }
        public int? CategoriaID { get; set; }
        public string Nome { get; set; } = null!;
        public string? CodigoBarras { get; set; }
        public decimal PrecoVenda { get; set; }
        public decimal? PrecoCusto { get; set; }
        public string UnidadeMedida { get; set; } = "UN";
        public DateTime? Validade { get; set; }
        public int EstoqueInicial { get; set; } = 0;
    }
}