namespace ninx.Domain.Entities
{
    public class Cargo
    {
        public int CargoID { get; set; }
        public string Nome { get; set; } = null!;
        public int Peso { get; set; }
        public int? ComercioID { get; set; }
        public bool Ativo { get; set; } = true;
        public bool Reservado { get; set; } = false;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }

        public Comercio? Comercio { get; set; }
        public ICollection<UsuarioComercio> UsuarioComercios { get; set; } = [];
    }
}
