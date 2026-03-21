using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

public class VendaMapping : IEntityTypeConfiguration<Venda>
{
    public void Configure(EntityTypeBuilder<Venda> builder)
    {
        builder.ToTable("Vendas");

        builder.HasKey(x => x.VendaID);

        builder.Property(x => x.VendaID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.DataHora)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.Total)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.TipoVenda)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion<string>()
            .HasDefaultValue(StatusVenda.Aberta);

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(x => x.Comercio)
            .WithMany(x => x.Vendas)
            .HasForeignKey(x => x.ComercioID)
            .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.Vendas)
            .HasForeignKey(x => x.UsuarioID)
            .OnDelete(DeleteBehavior.Restrict);

    }
}