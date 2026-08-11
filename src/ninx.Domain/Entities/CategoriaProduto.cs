namespace ninx.Domain.Entities
{
    public class CategoriaProduto
    {
        public int CategoriaID { get; set; }
        public int ComercioID { get; set; }
        public string Nome { get; set; }

        public Comercio Comercio { get; set; } = null!;
        public ICollection<Produto> Produtos { get; set; } = [];
    }
}
