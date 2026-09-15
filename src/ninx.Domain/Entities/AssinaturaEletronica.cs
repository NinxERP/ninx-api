using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class AssinaturaEletronica
    {   
        public int AssinaturaID { get; set; }
        /// <summary>Venda do documento; nulo quando o documento é um termo de abertura de conta.</summary>
        public int? VendaID { get; set; }

        /// <summary>Termo de abertura de conta; nulo quando o documento é de uma venda.</summary>
        public int? TermoAberturaID { get; set; }

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
        public Venda? Venda { get; set; }
        public TermoAberturaConta? TermoAbertura { get; set; }
        public PagamentoVenda? Pagamento { get; set; }
    }
}

