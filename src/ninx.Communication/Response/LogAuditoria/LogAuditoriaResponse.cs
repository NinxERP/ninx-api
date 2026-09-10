namespace ninx.Communication
{
    public class LogAuditoriaResponse
    {
        public int LogAuditoriaID { get; set; }
        public int UsuarioID { get; set; }
        public string UsuarioNome { get; set; } = string.Empty;
        public int? ComercioID { get; set; }
        public string Acao { get; set; } = string.Empty;
        public string Entidade { get; set; } = string.Empty;
        public int EntidadeID { get; set; }
        public string? Detalhes { get; set; }
        public DateTime CriadoEm { get; set; }
    }
}
