using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class Venda
    {
        public int VendaID { get; set; }
        public int ComercioID { get; set; }
        public int UsuarioID { get; set; }
        public int? ClienteID { get; set; }

        /// <summary>Quem comprou em nome do titular; nulo quando foi o próprio titular.</summary>
        public int? PessoaAutorizadaID { get; set; }
        public decimal Total { get; set; }
        public StatusVenda Status { get; set; } = StatusVenda.Aguardando;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }

        /// <summary>Só em venda fiada. Calculada na criação, para não mudar se o comércio alterar o dia depois.</summary>
        public DateTime? DataVencimento { get; set; }
        public TipoVenda TipoVenda { get; set; }
        public Comercio Comercio { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
        public Cliente? Cliente { get; set; } = null!;
        public PessoaAutorizada? PessoaAutorizada { get; set; }
        public ICollection<ItemVenda> ItensVenda { get; set; } = [];
        public ICollection<PagamentoVenda> PagamentosVenda { get; set; } = [];
        public virtual ICollection<AssinaturaEletronica> AssinaturasEletronicas { get; set; }
    }
}
