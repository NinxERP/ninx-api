using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Data.Mappings
{
    public class TermoAberturaContaMapping : IEntityTypeConfiguration<TermoAberturaConta>
    {
        public void Configure(EntityTypeBuilder<TermoAberturaConta> builder)
        {
            builder.ToTable("TermosAberturaConta");

            builder.HasKey(x => x.TermoAberturaID);

            builder.Property(x => x.TermoAberturaID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(12)
                .HasConversion<string>();

            builder.Property(x => x.LimiteCredito)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.CriadoEm)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.AssinadoEm)
                .IsRequired(false);

            builder.ToTable(t => t.HasCheckConstraint("CK_TermosAberturaConta_Status",
                "[Status] IN ('Aguardando', 'Ativo', 'Substituido', 'Cancelado')"));

            builder.HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.ClienteID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ClienteID, x.Status });
            builder.HasIndex(x => new { x.ClienteID, x.Versao }).IsUnique();
        }
    }
}
