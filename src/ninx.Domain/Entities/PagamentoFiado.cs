using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class PagamentoFiado
    {
        public int PagamentoID { get; set; }

        [Required]
        public int VendaFiadoID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "Valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        public DateTime DataHora { get; set; } = DateTime.Now;

        [MaxLength(200, ErrorMessage = "Observação deve ter no máximo 200 caracteres")]
        public string? Observacao { get; set; }

        public required VendaFiado VendaFiado { get; set; }
        public required Usuario Usuario { get; set; }
    }
}
