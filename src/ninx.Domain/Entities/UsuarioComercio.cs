using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class UsuarioComercio
    {
        public int UsuarioComercioID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [Required]
        public int ComercioID { get; set; }

        [EnumDataType(typeof(Permissao), ErrorMessage = "Permissão inválida")]
        public Permissao Permissao { get; set; }

        public bool Ativo { get; set; } = true;

        public required Usuario Usuario { get; set; }
        public required Comercio Comercio { get; set; }
    }
}
