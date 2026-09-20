namespace ninx.Communication
{
    public class PessoaAutorizadaRequest
    {
        public string Nome { get; set; } = null!;
        public string? Cpf { get; set; }

        /// <summary>1 = Cônjuge, 2 = Companheiro(a), 3 = Filho(a), 4 = Outro.</summary>
        public int Parentesco { get; set; }
        public bool MenorDeIdade { get; set; }
        public decimal? LimiteCredito { get; set; }
    }
}
