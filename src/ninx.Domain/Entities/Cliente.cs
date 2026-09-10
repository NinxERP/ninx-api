namespace ninx.Domain.Entities
{
    public class Cliente
    {
        public int ClienteID { get; set; }
        public int ComercioID { get; set; }
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
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }
        public Comercio Comercio { get; set; } = null!;
    }
}
