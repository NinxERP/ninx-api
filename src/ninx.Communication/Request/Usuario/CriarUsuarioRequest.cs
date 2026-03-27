namespace ninx.Communication.Request
{
    public class CriarUsuarioRequest
    {
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Senha { get; set; } = null!;
        public int Permissao { get; set; }
        public int ComercioId { get; set; }
    }
}