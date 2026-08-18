namespace ninx.Communication
{
    public class PagamentoHistoricoAssinaturaPlanoResponse
    {
        public int PagamentoAssinaturaID { get; set; }
        public int AssinaturaID { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataPagamento { get; set; }
        public DateTime DataVencimento { get; set; }
    }
}
