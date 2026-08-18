namespace ninx.Communication
{
    public class AtualizarMeuPerfilRequest
    {
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Senha { get; set; }
    }
}
