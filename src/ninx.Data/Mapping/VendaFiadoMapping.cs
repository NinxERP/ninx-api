using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

public class VendaFiadoMapping : IEntityTypeConfiguration<VendaFiado>
{
    public void Configure(EntityTypeBuilder<VendaFiado> builder)
    {
        builder.ToTable("VendasFiado");

        builder.HasKey(x => x.VendaFiadoID);

        builder.Property(x => x.VendaFiadoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(StatusFiado.Pendente);

        builder.Property(x => x.Assinatura)
            .IsRequired(false);

        builder.Property(x => x.DocumentoPDF)
            .IsRequired(false);

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.Venda)
            .WithOne(x => x.VendaFiado)
            .HasForeignKey<VendaFiado>(x => x.VendaID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Cliente)
            .WithMany(x => x.VendasFiado)
            .HasForeignKey(x => x.ClienteID);

        builder.HasMany(x => x.PagamentosFiado)
            .WithOne(x => x.VendaFiado)
            .HasForeignKey(x => x.VendaFiadoID);
    }
}