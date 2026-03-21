using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class UsuarioComercioMapping : IEntityTypeConfiguration<UsuarioComercio>
{
    public void Configure(EntityTypeBuilder<UsuarioComercio> builder)
    {
        builder.ToTable("UsuarioComercios");

        builder.HasKey(x => x.UsuarioComercioID);

        builder.Property(x => x.UsuarioComercioID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Permissao)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Ativo)
            .HasDefaultValue(true);

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.UsuarioComercios)
            .HasForeignKey(x => x.UsuarioID);

        builder.HasOne(x => x.Comercio)
            .WithMany(x => x.UsuarioComercios)
            .HasForeignKey(x => x.ComercioID);
    }
}