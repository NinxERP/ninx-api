using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class UsuarioComercio
    {
        public int UsuarioComercioID { get; set; }
        public int UsuarioID { get; set; }
        public int ComercioID { get; set; }
        public Permissao Permissao { get; set; }
        public bool Ativo { get; set; } = true;
        public string NomeComercio { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public Comercio Comercio { get; set; } = null!;
    }
}