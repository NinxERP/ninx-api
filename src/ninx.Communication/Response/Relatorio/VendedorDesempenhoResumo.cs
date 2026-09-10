namespace ninx.Communication
{
    public class VendedorDesempenhoResumo
    {
        public int UsuarioID { get; set; }
        public string UsuarioNome { get; set; } = null!;
        public int QuantidadeVendas { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal TicketMedio => QuantidadeVendas > 0 ? ValorTotal / QuantidadeVendas : 0;
    }
}
