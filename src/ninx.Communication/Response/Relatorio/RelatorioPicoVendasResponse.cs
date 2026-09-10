namespace ninx.Communication
{
    public class RelatorioPicoVendasResponse
    {
        public List<PicoPorHoraResumo> PorHora { get; set; } = new();
        public List<PicoPorDiaSemanaResumo> PorDiaSemana { get; set; } = new();
    }

    public class PicoPorHoraResumo
    {
        public int Hora { get; set; }
        public int QuantidadeVendas { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class PicoPorDiaSemanaResumo
    {
        public string DiaSemana { get; set; } = null!;
        public int QuantidadeVendas { get; set; }
        public decimal ValorTotal { get; set; }
    }
}
