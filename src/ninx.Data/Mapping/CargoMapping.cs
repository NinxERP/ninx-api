using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class CargoMapping : IEntityTypeConfiguration<Cargo>
{
    public void Configure(EntityTypeBuilder<Cargo> builder)
    {
        builder.ToTable("Cargos");

        builder.HasKey(x => x.CargoID);

        builder.Property(x => x.CargoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Peso)
            .IsRequired();

        builder.Property(x => x.Ativo)
            .HasDefaultValue(true);

        builder.Property(x => x.Reservado)
            .HasDefaultValue(false);

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.AtualizadoEm)
            .IsRequired(false);

        builder.HasIndex(x => new { x.ComercioID, x.Nome })
            .IsUnique()
            .HasFilter("[ComercioID] IS NOT NULL");

        builder.HasIndex(x => x.Nome)
            .IsUnique()
            .HasFilter("[ComercioID] IS NULL");

        builder.HasOne(x => x.Comercio)
            .WithMany()
            .HasForeignKey(x => x.ComercioID)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
