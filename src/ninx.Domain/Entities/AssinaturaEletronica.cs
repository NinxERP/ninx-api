using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class AssinaturaEletronica
    {   
        public int AssinaturaID { get; set; }
        public int VendaID { get; set; }

        /// <summary>Pagamento que este recibo comprova; nulo no termo de compromisso. Só passa a contar quando o recibo é assinado.</summary>
        public int? PagamentoID { get; set; }
        public Guid DocumentoGuid { get; set; }
        public TipoDocumento? TipoDocumento { get; set; }
        public string? DocumentoOriginalBase64 { get; set; }
        public string? DocumentoHtmlMesclado { get; set; }
        public string? DocumentoAssinadoBase64 { get; set; }
        public string? ImagemAssinatura { get; set; }

        /// <summary>SHA-256 (hexadecimal) dos bytes do documento apresentado ao signatário, calculado na emissão.</summary>
        public string? HashDocumentoOriginal { get; set; }

        /// <summary>SHA-256 (hexadecimal) dos bytes recebidos do signatário, calculado na confirmação.</summary>
        public string? HashDocumentoAssinado { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? DataAssinatura { get; set; }
        public string? IpAssinante { get; set; }
        public string? DispositivoInfo { get; set; }
        public bool Assinado { get; set; } = false;
        public StatusAssinatura Status { get; set; } = StatusAssinatura.Ativa;
        public Venda Venda { get; set; } = null!;
        public PagamentoVenda? Pagamento { get; set; }
    }
}

