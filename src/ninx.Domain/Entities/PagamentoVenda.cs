using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class PagamentoVenda
    {
        public int PagamentoID { get; set; }
        public int VendaID { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataHora { get; set; } = DateTime.Now;

        public Venda Venda { get; set; } = null!;
    }
}