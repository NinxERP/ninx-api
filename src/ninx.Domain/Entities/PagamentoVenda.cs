using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class PagamentoVenda
    {
        public int PagamentoID { get; set; }
        public int VendaID { get; set; }
        public int UsuarioID { get; set; }
        public int PagamentoVinculoID { get; set; }
        public FormaPagamento FormaPagamento { get; set; }
        public decimal Valor { get; set; }
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;
        public StatusPagamento Status { get; set; } = StatusPagamento.Pago;
        public Venda Venda { get; set; } = null!;
        
    }
}