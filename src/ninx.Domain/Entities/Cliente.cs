namespace ninx.Domain.Entities
{
    public class Cliente
    {
        public int ClienteID { get; set; }
        public int ComercioID { get; set; }
        public string Nome { get; set; } = null!;
        public string? Telefone { get; set; }
        public decimal? LimiteCredito { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
        public Comercio Comercio { get; set; } = null!;
        public ICollection<VendaFiado> VendasFiado { get; set; } = [];
    }
}