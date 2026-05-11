namespace ninx.Communication
{
    public class AssinaturaEletronicaResponse
    {
        public Guid DocumentoGuid { get; set; }
        public string NomeCliente { get; set; }
        public string NomeComercio { get; set; }
        public decimal ValorTotal { get; set; }
        public DateTime DataVenda { get; set; }
        public List<ItemVendaResumoResponse> Itens { get; set; }
    }

    public class ItemVendaResumoResponse
    {
        public string ProdutoNome { get; set; }
        public decimal Quantidade { get; set; }
        public decimal Subtotal { get; set; }
    }
}
