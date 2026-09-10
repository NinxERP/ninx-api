namespace ninx.Communication
{
    public class RelatorioComparativoComerciosResponse
    {
        public List<ComercioComparativoResumo> Comercios { get; set; } = new();
    }

    public class ComercioComparativoResumo
    {
        public int ComercioID { get; set; }
        public string ComercioNome { get; set; } = null!;
        public decimal FaturamentoTotal { get; set; }
        public int QuantidadeVendas { get; set; }
        public decimal TicketMedio => QuantidadeVendas > 0 ? FaturamentoTotal / QuantidadeVendas : 0;
    }
}
