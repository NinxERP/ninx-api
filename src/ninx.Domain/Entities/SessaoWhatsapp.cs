using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class SessaoWhatsapp
    {
        public int SessaoID { get; set; }
        public int ComercioID { get; set; }
        public string NumeroCelular { get; set; } = null!;
        public EtapaWhatsapp Etapa { get; set; } = EtapaWhatsapp.Menu;
        public string? DadosTemporarios { get; set; }
        public DateTime UltimaInteracao { get; set; } = DateTime.Now;

        public Comercio Comercio { get; set; } = null!;
    }
}