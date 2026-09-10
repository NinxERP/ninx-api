using ninx.Domain.Enums;

namespace ninx.Domain.Entities
{
    public class ItemVenda
    {
        public int ItemVendaID { get; set; }
        public int VendaID { get; set; }
        public int ProdutoID { get; set; }
        public string ProdutoNome { get; set; } = null!;
        public string? ProdutoCodigoBarras { get; set; }
        public UnidadeMedida UnidadeMedida { get; set; }
        public decimal Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }

        public Venda Venda { get; set; } = null!;
        public Produto Produto { get; set; } = null!;
    }
}