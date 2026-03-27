namespace ninx.Communication.Request
{
    public class ItemVendaRequest
    {
        public int ProdutoID { get; set; }
        public decimal Quantidade { get; set; }
        public string UnidadeMedida { get; set; } = string.Empty;
        public int ComercioId { get; set; }
    }
}
