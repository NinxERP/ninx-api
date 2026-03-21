using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class UsuarioMapping : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(x => x.UsuarioID);

        builder.Property(x => x.UsuarioID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SenhaHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.Permissao)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Ativo)
            .HasDefaultValue(true);

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETDATE()");

        builder.Property(x => x.AtualizadoEm)
            .IsRequired(false);
    }
}