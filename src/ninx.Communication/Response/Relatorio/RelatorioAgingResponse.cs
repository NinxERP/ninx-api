namespace ninx.Communication
{
    public class RelatorioAgingResponse
    {
        public AgingBucketResumo Ate30Dias { get; set; } = new();
        public AgingBucketResumo De31a60Dias { get; set; } = new();
        public AgingBucketResumo Acima60Dias { get; set; } = new();
        public List<VendaEmAbertoResumo> Vendas { get; set; } = new();
    }

    public class AgingBucketResumo
    {
        public decimal Valor { get; set; }
        public int Quantidade { get; set; }
    }

    public class VendaEmAbertoResumo
    {
        public int VendaID { get; set; }
        public int? ClienteID { get; set; }
        public string ClienteNome { get; set; } = null!;
        public decimal SaldoDevedor { get; set; }
        public DateTime CriadoEm { get; set; }
        public int DiasEmAberto { get; set; }
        public string Bucket => DiasEmAberto <= 30 ? "0-30" : DiasEmAberto <= 60 ? "31-60" : "60+";
    }
}
