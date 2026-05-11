namespace ninx.Domain.Entities
{
    public class PagamentoAssinaturaPlano
    {
        public int PagamentoAssinaturaID { get; set; }
        public int AssinaturaID { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; } = DateTime.UtcNow;
        public DateTime ProximoVencimento { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }

        public AssinaturaPlano Assinatura { get; set; } = null!;
    }
}
