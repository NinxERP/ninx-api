namespace ninx.Communication.Request
{
    public class ClienteRequest
    {
        public int ComercioID { get; set; }
        public string Nome { get; set; } = null!;
        public string? Telefone { get; set; }
        public decimal? LimiteCredito { get; set; }
    }
}
