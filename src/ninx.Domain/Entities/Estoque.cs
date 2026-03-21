namespace ninx.Domain.Entities
{
    public class Estoque
    {
        public int EstoqueID { get; set; }
        public int ProdutoID { get; set; }
        public int ComercioID { get; set; }
        public decimal Quantidade { get; set; } = 0;
        public decimal QuantidadeMinima { get; set; } = 0;
        public DateTime UltimaAtualizacao { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public Produto Produto { get; set; } = null!;
        public Comercio Comercio { get; set; } = null!;
    }
}