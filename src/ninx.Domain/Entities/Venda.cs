using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class Venda
    {
        public int VendaID { get; set; }
        public int ComercioID { get; set; }
        public int UsuarioID { get; set; }
        public decimal Total { get; set; }
        public StatusVenda Status { get; set; } = StatusVenda.Aberta;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public Comercio Comercio { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public ICollection<ItemVenda> ItensVenda { get; set; } = [];
        public ICollection<PagamentoVenda> PagamentosVenda { get; set; } = [];
        public VendaFiado? VendaFiado { get; set; }
    }
}