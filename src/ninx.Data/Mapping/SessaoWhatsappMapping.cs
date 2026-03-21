using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

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
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(x => x.DadosTemporarios)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(x => x.UltimaInteracao)
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(x => x.Comercio)
            .WithMany()
            .HasForeignKey(x => x.ComercioID);
    }
}