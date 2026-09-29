namespace ninx.Communication
{
    public class PessoaAutorizadaResponse
    {
        public int PessoaAutorizadaID { get; set; }
        public string Nome { get; set; } = null!;
        public string? Cpf { get; set; }
        public int Parentesco { get; set; }
        public bool MenorDeIdade { get; set; }
        public decimal? LimiteCredito { get; set; }

        /// <summary>Soma do que ainda está em aberto nas compras feitas por esta pessoa.</summary>
        public decimal SaldoDevedor { get; set; }

        /// <summary>
        /// Quanto a pessoa ainda pode dever: o menor entre o que resta do limite próprio, se houver,
        /// e o que a conta ainda comporta. O limite dela é um pedaço do limite da conta, não um a mais.
        /// </summary>
        public decimal? SaldoDisponivel { get; set; }
        public DateTime CriadoEm { get; set; }
        public DateTime? AutorizadaEm { get; set; }
        public DateTime? RevogacaoSolicitadaEm { get; set; }
        public DateTime? RevogadaEm { get; set; }

        /// <summary>"Pendente" (aguarda assinatura do termo), "Autorizada", "Revogação pendente" (ainda pode comprar até o titular assinar a nova versão) ou "Revogada".</summary>
        public string Situacao { get; set; } = null!;
    }
}
