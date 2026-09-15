using ninx.Communication.Venda;

namespace ninx.Communication
{
    public class CriarVendaRequest
    {
        public int ComercioID { get; set; }
        public int UsuarioID { get; set; }
        public int? ClienteID { get; set; }

        /// <summary>Pessoa autorizada que está comprando em nome do cliente, em venda fiada.</summary>
        public int? PessoaAutorizadaID { get; set; }
        public int TipoVenda { get; set; }
        public List<ItemVendaRequest> ItensVenda { get; set; } = new();
        public List<PagamentoVendaRequest> Pagamentos { get; set; } = new();
    }
}