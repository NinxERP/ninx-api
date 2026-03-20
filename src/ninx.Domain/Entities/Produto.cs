using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class Produto
    {
        public int ProdutoID { get; set; }

        [Required]
        public int ComercioID { get; set; }

        public int? CategoriaID { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public required string Nome { get; set; }

        [MaxLength(50, ErrorMessage = "Código de barras deve ter no máximo 50 caracteres")]
        public string? CodigoBarras { get; set; }

        [Required(ErrorMessage = "Preço de venda é obrigatório")]
        [Range(0.01, 99999.99, ErrorMessage = "Preço de venda deve ser maior que zero")]
        public decimal PrecoVenda { get; set; }

        [Range(0.01, 99999.99, ErrorMessage = "Preço de custo deve ser maior que zero")]
        public decimal? PrecoCusto { get; set; }

        [EnumDataType(typeof(UnidadeMedida), ErrorMessage = "Unidade de medida inválida")]
        public UnidadeMedida UnidadeMedida { get; set; } = UnidadeMedida.UN;

        public DateTime? Validade { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public required Comercio Comercio { get; set; }
        public CategoriaProduto? Categoria { get; set; }
        public Estoque? Estoque { get; set; }
        public ICollection<ItemVenda> ItensVenda { get; set; } = [];
        public ICollection<MovimentacaoEstoque> Movimentacoes { get; set; } = [];
    }
}
