namespace ninx.Communication
{
    public class ClienteRequest
    {
        public string Nome { get; set; } = null!;
        public string? Telefone { get; set; }
        public string Cpf { get; set; } = null!;
        public string? Email { get; set; }
        public string EnderecoLogradouro { get; set; } = null!;
        public string EnderecoNumero { get; set; } = null!;
        public string? EnderecoComplemento { get; set; }
        public string EnderecoBairro { get; set; } = null!;
        public string EnderecoCidade { get; set; } = null!;
        public string EnderecoUF { get; set; } = null!;
        public string EnderecoCEP { get; set; } = null!;
        public decimal? LimiteCredito { get; set; }
    }
}
