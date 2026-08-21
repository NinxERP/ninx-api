namespace ninx.Communication
{
    public class AtualizarUsuarioRequest
    {
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        // Opcional. Quando informado no PUT escopado por comércio, troca o cargo do vínculo do usuário nesse comércio.
        public int? CargoID { get; set; }
    }
}