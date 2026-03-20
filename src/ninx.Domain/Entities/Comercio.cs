using System.ComponentModel.DataAnnotations;

namespace ninx.Domain.Entities
{
    public class Comercio
    {
        public int ComercioID { get; set; }

        [Required(ErrorMessage = "Nome é obrigatório")]
        [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public required string Nome { get; set; }

        [MaxLength(200, ErrorMessage = "Endereço deve ter no máximo 200 caracteres")]
        public string? Endereco { get; set; }

        [MaxLength(18, ErrorMessage = "CNPJ deve ter no máximo 18 caracteres")]
        public string? CNPJ { get; set; }

        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }

        public ICollection<UsuarioComercio> UsuarioComercios { get; set; } = [];
        public ICollection<Produto> Produtos { get; set; } = [];
        public ICollection<Cliente> Clientes { get; set; } = [];
        public ICollection<Venda> Vendas { get; set; } = [];
        public ICollection<Assinatura> Assinaturas { get; set; } = [];
    }
}
