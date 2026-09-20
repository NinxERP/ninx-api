namespace ninx.Communication
{
    public class ContaFiadoResponse
    {
        public int ClienteID { get; set; }

        /// <summary>Há termo de abertura assinado: o cliente pode comprar fiado.</summary>
        public bool TermoAtivo { get; set; }

        /// <summary>Limite de crédito em vigor, o da última versão assinada.</summary>
        public decimal LimiteCredito { get; set; }

        /// <summary>Novo limite de uma versão ainda não assinada, quando difere do vigente.</summary>
        public decimal? LimitePendente { get; set; }
        public DateTime? TermoAssinadoEm { get; set; }
        public Guid? DocumentoGuidTermoAtivo { get; set; }

        /// <summary>Termo gerado e ainda não assinado, se houver.</summary>
        public Guid? DocumentoGuidTermoPendente { get; set; }

        /// <summary>Não há termo ativo, ou há inclusões ou revogações ainda não assinadas.</summary>
        public bool PrecisaNovoTermo { get; set; }
        public List<PessoaAutorizadaResponse> Autorizados { get; set; } = new();

        /// <summary>Todas as versões do termo, da mais nova para a mais antiga.</summary>
        public List<TermoAberturaResumoResponse> Termos { get; set; } = new();
    }

    public class TermoAberturaResumoResponse
    {
        public int Versao { get; set; }
        public decimal LimiteCredito { get; set; }

        /// <summary>"Aguardando", "Ativo", "Substituido" ou "Cancelado".</summary>
        public string Status { get; set; } = null!;
        public DateTime CriadoEm { get; set; }
        public DateTime? AssinadoEm { get; set; }
        public Guid? DocumentoGuid { get; set; }
    }
}
