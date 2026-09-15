namespace ninx.Communication
{
    public class PessoaAutorizadaResponse
    {
        public int PessoaAutorizadaID { get; set; }
        public string Nome { get; set; } = null!;
        public string? Cpf { get; set; }
        public int Parentesco { get; set; }
        public bool MenorDeIdade { get; set; }
        public decimal? LimitePorCompra { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? AutorizadaEm { get; set; }
        public DateTime? RevogacaoSolicitadaEm { get; set; }
        public DateTime? RevogadaEm { get; set; }

        /// <summary>"Pendente" (aguarda assinatura do termo), "Autorizada", "Revogação pendente" (ainda pode comprar até o titular assinar a nova versão) ou "Revogada".</summary>
        public string Situacao { get; set; } = null!;
    }
}
