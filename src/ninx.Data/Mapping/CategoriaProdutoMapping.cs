using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

namespace ninx.Data.Mappings
{
    public class CategoriaProdutoMapping : IEntityTypeConfiguration<CategoriaProduto>
    {
        public void Configure(EntityTypeBuilder<CategoriaProduto> builder)
        {
            builder.ToTable("CategoriasProduto");

            builder.HasKey(x => x.CategoriaID);

            builder.Property(x => x.CategoriaID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasMany(x => x.Produtos)
                .WithOne(x => x.Categoria)
                .HasForeignKey(x => x.CategoriaID);
        }
    }
}