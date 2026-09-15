namespace ninx.Domain.Entities
{
    public class Comercio
    {
        public int ComercioID { get; set; }
        public string NomeComercio { get; set; } = null!;
        public string? EnderecoLogradouro { get; set; }
        public string? EnderecoNumero { get; set; }
        public string? EnderecoComplemento { get; set; }
        public string? EnderecoBairro { get; set; }
        public string? EnderecoCidade { get; set; }
        public string? EnderecoUF { get; set; }
        public string? EnderecoCEP { get; set; }
        public string? CNPJ { get; set; }
        public string? AssinaturaResponsavelBase64 { get; set; }
        public decimal? LimiteCreditoPadrao { get; set; }

        /// <summary>Dia do mês (1 a 31) em que vencem as vendas fiadas. Ver <c>VencimentoFiado</c>.</summary>
        public int DiaVencimentoFiado { get; set; } = Regras.VencimentoFiado.DiaPadrao;
        public bool Ativo { get; set; } = true;
        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public DateTime? AtualizadoEm { get; set; }

        public ICollection<UsuarioComercio> UsuarioComercios { get; set; } = [];
        public ICollection<Produto> Produtos { get; set; } = [];
        public ICollection<Cliente> Clientes { get; set; } = [];
        public ICollection<Venda> Vendas { get; set; } = [];
        public ICollection<AssinaturaPlano> Assinaturas { get; set; } = [];
        public ICollection<CategoriaProduto> CategoriasProduto { get; set; } = [];
    }
}