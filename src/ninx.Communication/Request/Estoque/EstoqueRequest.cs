namespace ninx.Communication
{
    public class EstoqueRequest
    {
        public int ProdutoID { get; set; }
        public decimal Quantidade { get; set; }
        public decimal QuantidadeMinima { get; set; }
    }
}
