namespace ninx.Communication.Request.Login
{
    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Senha { get; set; } = null!;
        public int? ComercioID { get; set; }
    }
}