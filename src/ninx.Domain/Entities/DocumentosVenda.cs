using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class DocumentosVenda
    {   
        public int AssinaturaID { get; set; }
        public string DocumentoNome { get; set; }
        public int VendaID { get; set; }
        public Guid DocumentoGuid { get; set; }
        public string? ImagemAssinatura { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? DataAssinatura { get; set; }
        public string? IpAssinante { get; set; }
        public string? DispositivoInfo { get; set; }
        public bool Assinado { get; set; } = false;
        public StatusAssinaturaEletronica Status { get; set; } = StatusAssinaturaEletronica.Pendente;
        public Venda Venda { get; set; } = null!;
    }
}

