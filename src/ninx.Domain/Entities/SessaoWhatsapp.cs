using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class SessaoWhatsapp
    {
        public int SessaoID { get; set; }

        [Required]
        public int ComercioID { get; set; }

        [Required(ErrorMessage = "Número de celular é obrigatório")]
        [MaxLength(20)]
        [Phone(ErrorMessage = "Número de celular inválido")]
        public required string NumeroCelular { get; set; }

        [EnumDataType(typeof(EtapaWhatsapp), ErrorMessage = "Etapa inválida")]
        public EtapaWhatsapp Etapa { get; set; } = EtapaWhatsapp.Menu;

        [MaxLength(500)]
        public string? DadosTemporarios { get; set; }

        public DateTime UltimaInteracao { get; set; } = DateTime.Now;

        public required Comercio Comercio { get; set; }
    }
}
