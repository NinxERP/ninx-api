namespace ninx.Communication
{
    public class ComercioResponse
    {
        public int ComercioID { get; set; }
        public string? NomeComercio { get; set; }
        public string? Endereco { get; set; }
        public string? CNPJ { get; set; }
        public string? AssinaturaResponsavelBase64 { get; set; }
        public decimal? LimiteCreditoPadrao { get; set; }
    }
}
