using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class Cliente
    {
        public int ClienteID { get; set; }

        [Required]
        public int ComercioID { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public required string Nome { get; set; }

        [MaxLength(20, ErrorMessage = "Telefone deve ter no máximo 20 caracteres")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string? Telefone { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Limite de crédito não pode ser negativo")]
        public decimal? LimiteCredito { get; set; }

        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public required Comercio Comercio { get; set; }
        public ICollection<VendaFiado> VendasFiado { get; set; } = [];
    }
}
