using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class ComercioMapping : IEntityTypeConfiguration<Comercio>
{
    public void Configure(EntityTypeBuilder<Comercio> builder)
    {
        builder.ToTable("Comercios");

        builder.HasKey(x => x.ComercioID);

        builder.Property(x => x.ComercioID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NomeComercio)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Endereco)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(x => x.CNPJ)
            .IsRequired(false)
            .HasMaxLength(18);

        builder.HasIndex(x => x.CNPJ)
            .IsUnique();

        builder.Property(x => x.Ativo)
            .HasDefaultValue(true);

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.AtualizadoEm)
            .IsRequired(false);
    }
}