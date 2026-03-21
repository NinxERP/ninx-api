using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class EstoqueMapping : IEntityTypeConfiguration<Estoque>
{
    public void Configure(EntityTypeBuilder<Estoque> builder)
    {
        builder.ToTable("Estoque");

        builder.HasKey(x => x.EstoqueID);

        builder.Property(x => x.EstoqueID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Quantidade)
            .IsRequired()
            .HasColumnType("decimal(10,3)")
            .HasDefaultValue(0);

        builder.Property(x => x.QuantidadeMinima)
            .HasColumnType("decimal(10,3)")
            .HasDefaultValue(0);

        builder.Property(x => x.UltimaAtualizacao)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.AtualizadoEm)
            .IsRequired(false);

        // Unique constraint: mesmo produto não pode ter dois registros no mesmo comércio
        builder.HasIndex(x => new { x.ProdutoID, x.ComercioID })
            .IsUnique();

        builder.HasOne(x => x.Produto)
            .WithOne(x => x.Estoque)
            .HasForeignKey<Estoque>(x => x.ProdutoID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Comercio)
            .WithMany()
            .HasForeignKey(x => x.ComercioID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}