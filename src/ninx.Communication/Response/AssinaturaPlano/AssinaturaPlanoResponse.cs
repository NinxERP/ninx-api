namespace ninx.Communication
{
    public class AssinaturaPlanoResponse
    {
        public int ComercioID { get; set; }
        public string Plano { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }

    }
}
