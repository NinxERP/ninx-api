using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class VendaFiado
    {
        public int VendaFiadoID { get; set; }
        public int VendaID { get; set; }
        public int ClienteID { get; set; }
        public StatusFiado Status { get; set; } = StatusFiado.Pendente;
        public byte[]? Assinatura { get; set; }
        public byte[]? DocumentoPDF { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public Venda Venda { get; set; } = null!;
        public Cliente Cliente { get; set; } = null!;
        public ICollection<PagamentoFiado> PagamentosFiado { get; set; } = [];
    }
}