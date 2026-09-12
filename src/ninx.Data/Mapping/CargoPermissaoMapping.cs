using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class CargoPermissaoMapping : IEntityTypeConfiguration<CargoPermissao>
{
    public void Configure(EntityTypeBuilder<CargoPermissao> builder)
    {
        builder.ToTable("CargosPermissoes");

        builder.HasKey(x => x.CargoPermissaoID);

        builder.Property(x => x.CargoPermissaoID)
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => new { x.CargoID, x.PermissaoID })
            .IsUnique();

        builder.HasOne(x => x.Cargo)
            .WithMany(x => x.CargoPermissoes)
            .HasForeignKey(x => x.CargoID)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permissao)
            .WithMany(x => x.CargoPermissoes)
            .HasForeignKey(x => x.PermissaoID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
