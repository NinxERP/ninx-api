namespace ninx.Communication.Response
{
    public class UsuarioResponse
    {
        public int UsuarioID { get; set; }
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Permissao { get; set; } = null!;
        public bool Ativo { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}