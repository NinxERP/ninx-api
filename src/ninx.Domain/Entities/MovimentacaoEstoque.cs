namespace ninx.Domain.Entities
{
    public class MovimentacaoEstoque
    {
        public int MovimentacaoID { get; set; }
        public int ComercioID { get; set; }
        public int ProdutoID { get; set; }
        public int UsuarioID { get; set; }
        public string Tipo { get; set; } = null!;
        public decimal Quantidade { get; set; }
        public int? VendaID { get; set; } 
        public string? Observacao { get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;

        public Produto Produto { get; set; } = null!;
    }
}