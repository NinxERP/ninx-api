using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class Usuario
    {
        public int UsuarioID { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public required string Nome { get; set; }

        [Required(ErrorMessage = "Login é obrigatório")]
        [MaxLength(50, ErrorMessage = "Login deve ter no máximo 50 caracteres")]
        public required string Login { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        [MaxLength(255)]
        public required string SenhaHash { get; set; }

        [EnumDataType(typeof(Permissao), ErrorMessage = "Permissão inválida")]
        public Permissao Permissao { get; set; }

        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public ICollection<UsuarioComercio> UsuarioComercios { get; set; } = [];
        public ICollection<Venda> Vendas { get; set; } = [];
        public ICollection<PagamentoFiado> PagamentosFiado { get; set; } = [];
    }
}
