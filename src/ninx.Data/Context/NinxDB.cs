using Microsoft.EntityFrameworkCore;
using ninx.Domain.Entities;

namespace ninx.Data.Context
{
    public class NinxDB : DbContext
    {
        public NinxDB(DbContextOptions<NinxDB> options)
            : base(options)
        {
        }

        public DbSet<AssinaturaPlano> Assinaturas { get; set; }
        public DbSet<Comercio> Comercio { get; set; }
        public DbSet<CategoriaProduto> CategoriaProduto { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Estoque> Estoques { get; set; }
        public DbSet<ItemVenda> ItemVendas { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacaoEstoque { get; set; }
        public DbSet<PagamentoVenda> PagamentoVendas { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<SessaoWhatsapp> SessaoWhatsapps { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }    
        public DbSet<UsuarioComercio> UsuarioComercio { get; set; }
        public DbSet<Venda> Vendas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(NinxDB).Assembly);
        }
    }
}
