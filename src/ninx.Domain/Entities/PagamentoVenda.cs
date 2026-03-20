using ninx.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class PagamentoVenda
    {
        public int PagamentoID { get; set; }

        [Required]
        public int VendaID { get; set; }

        [EnumDataType(typeof(FormaPagamento), ErrorMessage = "Forma de pagamento inválida")]
        public FormaPagamento FormaPagamento { get; set; }

        [Required(ErrorMessage = "Valor é obrigatório")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Valor deve ser maior que zero")]
        public decimal Valor { get; set; }

        public DateTime DataHora { get; set; } = DateTime.Now;

        public required Venda Venda { get; set; }
    }
}
