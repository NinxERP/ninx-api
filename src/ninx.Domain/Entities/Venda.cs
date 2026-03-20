using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class Venda
    {
        public int VendaID { get; set; }

        [Required]
        public int ComercioID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        public DateTime DataHora { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Total é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total deve ser maior que zero")]
        public decimal Total { get; set; }

        [EnumDataType(typeof(TipoVenda), ErrorMessage = "Tipo de venda inválido")]
        public TipoVenda TipoVenda { get; set; }

        [EnumDataType(typeof(StatusVenda), ErrorMessage = "Status inválido")]
        public StatusVenda Status { get; set; } = StatusVenda.Aberta;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public required Comercio Comercio { get; set; }
        public required Usuario Usuario { get; set; }
        public ICollection<ItemVenda> Itens { get; set; } = [];
        public ICollection<PagamentoVenda> Pagamentos { get; set; } = [];
        public VendaFiado? VendaFiado { get; set; }
    }
}
