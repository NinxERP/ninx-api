namespace ninx.Communication
{
    public class AssinaturaPlanoResponse
    {
        public int ComercioID { get; set; }
        public int Plano { get; set; }
        public int Status { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }

    }
}
