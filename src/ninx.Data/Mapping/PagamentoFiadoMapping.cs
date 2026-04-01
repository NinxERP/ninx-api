using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class PagamentoFiadoMapping : IEntityTypeConfiguration<PagamentoFiado>
{
    public void Configure(EntityTypeBuilder<PagamentoFiado> builder)
    {
        builder.ToTable("PagamentosFiado");

        builder.HasKey(x => x.PagamentoID);

        builder.Property(x => x.PagamentoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Valor)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.DataHora)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.Observacao)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.HasOne(x => x.VendaFiado)
            .WithMany(x => x.PagamentosFiado)
            .HasForeignKey(x => x.VendaFiadoID);

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.PagamentosFiado)
            .HasForeignKey(x => x.UsuarioID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}