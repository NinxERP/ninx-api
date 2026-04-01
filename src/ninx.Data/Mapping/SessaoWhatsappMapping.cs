using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

public class SessaoWhatsappMapping : IEntityTypeConfiguration<SessaoWhatsapp>
{
    public void Configure(EntityTypeBuilder<SessaoWhatsapp> builder)
    {
        builder.ToTable("SessoesWhatsapp");

        builder.HasKey(x => x.SessaoID);

        builder.Property(x => x.SessaoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NumeroCelular)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Etapa)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(EtapaWhatsapp.Menu);

        builder.Property(x => x.DadosTemporarios)
            .IsRequired(false);

        builder.Property(x => x.UltimaInteracao)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(x => new { x.ComercioID, x.NumeroCelular })
            .IsUnique();

        builder.HasOne(x => x.Comercio)
            .WithMany()
            .HasForeignKey(x => x.ComercioID);
    }
}