namespace ninx.Communication
{
    public class ComercioResponse
    {
        public int ComercioID { get; set; }
        public string? NomeComercio { get; set; }
        public string? Endereco { get; set; }
        public string? EnderecoLogradouro { get; set; }
        public string? EnderecoNumero { get; set; }
        public string? EnderecoComplemento { get; set; }
        public string? EnderecoBairro { get; set; }
        public string? EnderecoCidade { get; set; }
        public string? EnderecoUF { get; set; }
        public string? EnderecoCEP { get; set; }
        public string? CNPJ { get; set; }
        public string? AssinaturaResponsavelBase64 { get; set; }
        public decimal? LimiteCreditoPadrao { get; set; }
    }
}
