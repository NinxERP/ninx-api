using ninx.Communication.Request.Venda;

namespace ninx.Communication.Request
{
    public class CriarVendaRequest
    {
        public int ComercioID { get; set; }
        public int UsuarioID { get; set; }
        public int? ClienteID { get; set; }
        public string? Observacoes { get; set; }
        public int TipoVenda { get; set; }
        public List<ItemVendaRequest> ItensVenda { get; set; } = new();
        public List<PagamentoVendaRequest> Pagamentos { get; set; } = new();
    }
}