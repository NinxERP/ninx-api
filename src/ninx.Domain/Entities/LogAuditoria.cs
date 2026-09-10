namespace ninx.Domain.Entities
{
    public class LogAuditoria
    {
        public int LogAuditoriaID { get; set; }
        public int UsuarioID { get; set; }
        public int? ComercioID { get; set; }
        public string Acao { get; set; } = null!;
        public string Entidade { get; set; } = null!;
        public int EntidadeID { get; set; }
        public string? Detalhes { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public Usuario Usuario { get; set; } = null!;
        public Comercio? Comercio { get; set; }
    }
}
