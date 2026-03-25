namespace ninx.Communication.Response
{
    public class LoginResponse
    {
        public string? Token { get; set; }
        public List<ComercioSimplificado>? Comercios { get; set; }
    }

    public class ComercioSimplificado
    {
        public int ComercioID { get; set; }
        public string Nome { get; set; } = null!;
    }
}
