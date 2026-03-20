using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class ItemVenda
    {
        public int ItemVendaID { get; set; }

        [Required]
        public int VendaID { get; set; }

        [Required]
        public int ProdutoID { get; set; }

        [Required(ErrorMessage = "Nome do produto é obrigatório")]
        [MaxLength(100)]
        public required string ProdutoNome { get; set; }

        [MaxLength(50)]
        public string? ProdutoCodigoBarras { get; set; }

        [EnumDataType(typeof(UnidadeMedida), ErrorMessage = "Unidade de medida inválida")]
        public UnidadeMedida UnidadeMedida { get; set; }

        [Required(ErrorMessage = "Quantidade é obrigatória")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantidade deve ser maior que zero")]
        public decimal Quantidade { get; set; }

        [Required(ErrorMessage = "Preço unitário é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Preço unitário deve ser maior que zero")]
        public decimal PrecoUnitario { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Subtotal deve ser maior que zero")]
        public decimal Subtotal { get; set; }

        public required Venda Venda { get; set; }
        public required Produto Produto { get; set; }
    }
}
