using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class Assinatura
    {
        public int AssinaturaID { get; set; }

        [Required]
        public int ComercioID { get; set; }

        [EnumDataType(typeof(PlanoAssinatura), ErrorMessage = "Plano inválido")]
        public PlanoAssinatura Plano { get; set; }

        [Required(ErrorMessage = "Data de início é obrigatória")]
        public DateTime DataInicio { get; set; }

        [Required(ErrorMessage = "Data de fim é obrigatória")]
        public DateTime DataFim { get; set; }

        [EnumDataType(typeof(StatusAssinatura), ErrorMessage = "Status inválido")]
        public StatusAssinatura Status { get; set; } = StatusAssinatura.Ativa;

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public required Comercio Comercio { get; set; }
    }
}
