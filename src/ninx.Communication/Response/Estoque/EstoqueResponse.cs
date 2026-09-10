namespace ninx.Communication
{
    public class EstoqueResponse
    {
        public int EstoqueID { get; set; }
        public int ProdutoID { get; set; }
        public int ComercioID { get; set; }
        public decimal Quantidade { get; set; }
        public decimal QuantidadeMinima { get; set; }
        public DateTime UltimaAtualizacao { get; set; }
        public DateTime? AtualizadoEm { get; set; }
    }
}
