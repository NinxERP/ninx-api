namespace ninx.Communication.Request
{
    public class ItemVendaRequest
    {
        public int ProdutoID { get; set; }
        public decimal Quantidade { get; set; }
        public decimal? PrecoUnitario { get; set; }
        public string UnidadeMedida { get; set; } = string.Empty;

    }
}
