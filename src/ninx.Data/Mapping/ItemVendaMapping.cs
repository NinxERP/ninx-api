using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class ItemVendaMapping : IEntityTypeConfiguration<ItemVenda>
{
    public void Configure(EntityTypeBuilder<ItemVenda> builder)
    {
        builder.ToTable("ItensVenda");

        builder.HasKey(x => x.ItemVendaID);

        builder.Property(x => x.ItemVendaID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.ProdutoNome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ProdutoCodigoBarras)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(x => x.UnidadeMedida)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion<string>();

        builder.Property(x => x.Quantidade)
            .IsRequired()
            .HasColumnType("decimal(10,3)");

        builder.Property(x => x.PrecoUnitario)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.Subtotal)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.HasOne(x => x.Venda)
            .WithMany(x => x.ItensVenda)
            .HasForeignKey(x => x.VendaID)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.Produto)
            .WithMany()
            .HasForeignKey(x => x.ProdutoID)
            .OnDelete(DeleteBehavior.Restrict);

    }
}