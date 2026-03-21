using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class MovimentacaoEstoque
    {
        public int MovimentacaoID { get; set; }
        public int ComercioID { get; set; }
        public int ProdutoID { get; set; }
        public int UsuarioID { get; set; }
        public TipoMovimentacao Tipo { get; set; }
        public decimal Quantidade { get; set; }
        public int? ReferenciaID { get; set; }
        public string? Observacao { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;

        public Comercio Comercio { get; set; } = null!;
        public Produto Produto { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
}