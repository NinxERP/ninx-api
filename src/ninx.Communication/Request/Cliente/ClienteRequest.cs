namespace ninx.Communication
{
    public class ClienteRequest
    {
        public string Nome { get; set; } = null!;
        public string? Telefone { get; set; }
        public decimal? LimiteCredito { get; set; }
    }
}
