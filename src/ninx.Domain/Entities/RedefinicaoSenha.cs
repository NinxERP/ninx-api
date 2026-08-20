namespace ninx.Domain.Entities
{
    public class RedefinicaoSenha
    {
        public int RedefinicaoSenhaID { get; set; }
        public int UsuarioID { get; set; }
        public string CodigoHash { get; set; } = null!;
        public int Tentativas { get; set; } = 0;
        public bool Utilizado { get; set; } = false;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime ExpiraEm { get; set; }

        public Usuario Usuario { get; set; } = null!;
    }
}
