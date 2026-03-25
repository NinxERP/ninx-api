namespace ninx.Communication.Request
{
    public class AtualizarProdutoRequest
    {
        public int? CategoriaID { get; set; }
        public string Nome { get; set; } = null!;
        public string? CodigoBarras { get; set; }
        public decimal PrecoVenda { get; set; }
        public decimal? PrecoCusto { get; set; }
        public string UnidadeMedida { get; set; } = "UN";
        public DateTime? Validade { get; set; }
    }
}