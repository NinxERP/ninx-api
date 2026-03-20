using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class MovimentacaoEstoque
    {
        public int MovimentacaoID { get; set; }

        [Required]
        public int ComercioID { get; set; }

        [Required]
        public int ProdutoID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [EnumDataType(typeof(TipoMovimentacao), ErrorMessage = "Tipo de movimentação inválido")]
        public TipoMovimentacao Tipo { get; set; }

        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        public int? ReferenciaID { get; set; }

        [MaxLength(200, ErrorMessage = "Observação deve ter no máximo 200 caracteres")]
        public string? Observacao { get; set; }

        public DateTime DataHora { get; set; } = DateTime.Now;

        public required Comercio Comercio { get; set; }
        public required Produto Produto { get; set; }
        public required Usuario Usuario { get; set; }
    }
}
