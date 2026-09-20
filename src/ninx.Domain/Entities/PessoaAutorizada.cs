using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    /// <summary>
    /// Pessoa que o titular autoriza a comprar fiado em sua conta. Só pode comprar depois que um
    /// termo de abertura que a lista for assinado (<see cref="AutorizadaEm"/> preenchido). A
    /// revogação também exige assinatura: é pedida (<see cref="RevogacaoSolicitadaEm"/>) e só vale
    /// quando o titular assina o termo seguinte, que já não lista a pessoa.
    /// </summary>
    public class PessoaAutorizada
    {
        public int PessoaAutorizadaID { get; set; }
        public int ClienteID { get; set; }
        public string Nome { get; set; } = null!;
        public string? Cpf { get; set; }
        public ParentescoAutorizado Parentesco { get; set; }
        public bool MenorDeIdade { get; set; }
        /// <summary>
        /// Limite de crédito próprio da pessoa, somado sobre as compras dela ainda em aberto.
        /// Nulo: compra dentro do limite da conta, sem teto individual.
        /// </summary>
        public decimal? LimiteCredito { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AutorizadaEm { get; set; }
        public DateTime? RevogacaoSolicitadaEm { get; set; }
        public DateTime? RevogadaEm { get; set; }
        public Cliente Cliente { get; set; } = null!;

        public bool PodeComprar => AutorizadaEm.HasValue && !RevogadaEm.HasValue;
    }
}
