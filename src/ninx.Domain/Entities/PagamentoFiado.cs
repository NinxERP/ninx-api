namespace ninx.Domain.Entities
{
    public class PagamentoFiado
    {
        public int PagamentoID { get; set; }
        public int VendaFiadoID { get; set; }
        public int UsuarioID { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataHora { get; set; } = DateTime.UtcNow;
        public string? Observacao { get; set; }

        public VendaFiado VendaFiado { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
}