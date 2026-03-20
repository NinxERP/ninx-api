using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class VendaFiado
    {
        public int VendaFiadoID { get; set; }

        [Required]
        public int VendaID { get; set; }

        [Required]
        public int ClienteID { get; set; }

        [EnumDataType(typeof(StatusFiado), ErrorMessage = "Status inválido")]
        public StatusFiado Status { get; set; } = StatusFiado.Pendente;

        public byte[]? Assinatura { get; set; }
        public byte[]? DocumentoPDF { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public required Venda Venda { get; set; }
        public required Cliente Cliente { get; set; }
        public ICollection<PagamentoFiado> Pagamentos { get; set; } = [];
    }
}
