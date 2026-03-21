using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

public class ProdutoMapping : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");

        builder.HasKey(x => x.ProdutoID);

        builder.Property(x => x.ProdutoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CodigoBarras)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.PrecoVenda)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.PrecoCusto)
            .IsRequired(false)
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.UnidadeMedida)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion<string>()
            .HasDefaultValue(UnidadeMedida.UN);

        builder.Property(x => x.Validade)
            .IsRequired(false)
            .HasColumnType("date");

        builder.Property(x => x.Ativo)
            .HasDefaultValue(true);

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.AtualizadoEm)
            .IsRequired(false);

        builder.HasIndex(x => new { x.ComercioID, x.CodigoBarras })
            .IsUnique()
            .HasFilter("[CodigoBarras] IS NOT NULL");

        builder.HasOne(x => x.Comercio)
            .WithMany(x => x.Produtos)
            .HasForeignKey(x => x.ComercioID);

        builder.HasOne(x => x.Categoria)
            .WithMany(x => x.Produtos)
            .HasForeignKey(x => x.CategoriaID);
    }
}