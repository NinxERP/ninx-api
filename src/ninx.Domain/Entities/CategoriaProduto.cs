using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class CategoriaProduto
    {
        public int CategoriaID { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public required string Nome { get; set; }

        public ICollection<Produto> Produtos { get; set; } = [];
    }
}
