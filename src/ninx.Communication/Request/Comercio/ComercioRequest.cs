namespace ninx.Communication.Request
{
    public class ComercioRequest
    {
        public string Nome { get; set; } = null!;
        public string? Endereco { get; set; }
        public string? CNPJ { get; set; }
    }
}
