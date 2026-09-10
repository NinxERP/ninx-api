namespace ninx.Communication
{
    public class ClienteLimiteCreditoResumo
    {
        public int ClienteID { get; set; }
        public string ClienteNome { get; set; } = null!;
        public decimal LimiteCredito { get; set; }
        public decimal SaldoDevedor { get; set; }
        public decimal LimiteDisponivel => Math.Max(LimiteCredito - SaldoDevedor, 0);
        public decimal PercentualUtilizado => LimiteCredito > 0 ? Math.Round(SaldoDevedor / LimiteCredito * 100, 2) : 0;
    }
}
