namespace ninx.Communication.Response.Login
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
