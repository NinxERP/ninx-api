namespace ninx.Communication
{
    public class CriarCargoRequest
    {
        public string Nome { get; set; } = null!;
        public List<int> PermissaoIds { get; set; } = [];
        // null = cargo base (global, restrito a administradores de plataforma); valor = cargo customizado desse comércio.
        public int? ComercioID { get; set; }
    }
}
