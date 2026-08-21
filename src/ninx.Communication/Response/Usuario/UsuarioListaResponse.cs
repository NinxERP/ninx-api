namespace ninx.Communication
{
    public class UsuarioListaResponse
    {
        public int UsuarioID { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
        public int ComercioID { get; set; }
        public string ComercioNome { get; set; } = null!;
        public string CargoNome { get; set; } = null!;
    }
}
