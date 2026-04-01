using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class Assinatura
    {
        public int AssinaturaID { get; set; }
        public int ComercioID { get; set; }
        public PlanoAssinatura Plano { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public StatusAssinatura Status { get; set; } = StatusAssinatura.Ativa;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }
        public Comercio Comercio { get; set; } = null!;
    }
}