using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class Produto
    {
        public int ProdutoID { get; set; }
        public int ComercioID { get; set; }
        public int? CategoriaID { get; set; }
        public string Nome { get; set; } = null!;
        public string? CodigoBarras { get; set; }
        public decimal PrecoVenda { get; set; }
        public decimal? PrecoCusto { get; set; }
        public UnidadeMedida UnidadeMedida { get; set; } = UnidadeMedida.UN;
        public DateTime? Validade { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public Comercio Comercio { get; set; } = null!;
        public CategoriaProduto? Categoria { get; set; }
        public Estoque? Estoque { get; set; }

    }
}