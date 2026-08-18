using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class LogAuditoriaMapping : IEntityTypeConfiguration<LogAuditoria>
{
    public void Configure(EntityTypeBuilder<LogAuditoria> builder)
    {
        builder.ToTable("LogsAuditoria");

        builder.HasKey(x => x.LogAuditoriaID);

        builder.Property(x => x.LogAuditoriaID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Acao)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Entidade)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Detalhes)
            .HasMaxLength(1000);

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioID);

        builder.HasOne(x => x.Comercio)
            .WithMany()
            .HasForeignKey(x => x.ComercioID)
            .IsRequired(false);

        builder.HasIndex(x => x.CriadoEm);
    }
}
