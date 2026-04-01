namespace ninx.Domain.Entities
{
    public class CategoriaProduto
    {
        public int CategoriaID { get; set; }
        public required string Nome { get; set; }

        public ICollection<Produto> Produtos { get; set; } = [];
    }
}
