using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Nome { get; set; } = null!;
        public string SenhaHash { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Permissao Permissao { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }

        public ICollection<UsuarioComercio> UsuarioComercios { get; set; } = [];
        public ICollection<Venda> Vendas { get; set; } = [];
        public ICollection<PagamentoFiado> PagamentosFiado { get; set; } = [];
    }
}