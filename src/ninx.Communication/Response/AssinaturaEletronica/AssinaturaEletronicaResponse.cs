namespace ninx.Communication
{
    public class AssinaturaEletronicaResponse
    {
        public Guid DocumentoGuid { get; set; }
        public string DocumentoBase64 { get; set; }
    }

    public class ItemVendaResumoResponse
    {
        public string ProdutoNome { get; set; }
        public decimal Quantidade { get; set; }
        public decimal Subtotal { get; set; }
    }
}
