namespace ninx.Communication
{
    public class ConfirmarRedefinicaoSenhaRequest
    {
        public string Email { get; set; } = null!;
        public string Codigo { get; set; } = null!;
        public string NovaSenha { get; set; } = null!;
    }
}
