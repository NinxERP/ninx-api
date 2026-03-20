using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class Estoque
    {
        public int EstoqueID { get; set; }

        [Required]
        public int ProdutoID { get; set; }

        [Required]
        public int ComercioID { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Quantidade não pode ser negativa")]
        public decimal Quantidade { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Quantidade mínima não pode ser negativa")]
        public decimal QuantidadeMinima { get; set; } = 0;

        public DateTime UltimaAtualizacao { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public required Produto Produto { get; set; }
        public required Comercio Comercio { get; set; }
    }
}
