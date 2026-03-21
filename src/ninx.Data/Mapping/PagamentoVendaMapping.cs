using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class PagamentoVendaMapping : IEntityTypeConfiguration<PagamentoVenda>
{
    public void Configure(EntityTypeBuilder<PagamentoVenda> builder)
    {
        builder.ToTable("PagamentosVenda");

        builder.HasKey(x => x.PagamentoID);

        builder.Property(x => x.PagamentoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.FormaPagamento)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Valor)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.DataHora)
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(x => x.Venda)
            .WithMany(x => x.PagamentosVenda)
            .HasForeignKey(x => x.VendaID)
            .OnDelete(DeleteBehavior.Restrict);

    }
}