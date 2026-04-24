namespace ninx.Domain.Entities
{
    public class Comercio
    {
        public int ComercioID { get; set; }
        public string NomeComercio { get; set; } = null!;
        public string? Endereco { get; set; }
        public string? CNPJ { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }

        public ICollection<UsuarioComercio> UsuarioComercios { get; set; } = [];
        public ICollection<Produto> Produtos { get; set; } = [];
        public ICollection<Cliente> Clientes { get; set; } = [];
        public ICollection<Venda> Vendas { get; set; } = [];
        public ICollection<AssinaturaPlano> Assinaturas { get; set; } = [];
    }
}