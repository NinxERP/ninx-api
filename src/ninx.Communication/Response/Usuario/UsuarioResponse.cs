namespace ninx.Communication
{
    public class UsuarioResponse
    {
        public int UsuarioID { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool Admin { get; set; }
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
        // Só preenchido pelos endpoints escopados por comércio (GET /api/Usuario/All e GET /api/Usuario/{id}): permissão do vínculo do usuário nesse comércio.
        public string? Permissao { get; set; }
    }
}