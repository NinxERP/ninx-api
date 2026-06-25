using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Data.Mappings
{
    public class DocumentosVendaMapping : IEntityTypeConfiguration<DocumentosVenda>
    {
        public void Configure(EntityTypeBuilder<DocumentosVenda> builder)
        {
            builder.ToTable("DocumentosVida");

            builder.HasKey(x => x.AssinaturaID);

            builder.Property(x => x.AssinaturaID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.DocumentoGuid)
                .IsRequired();

            builder.Property(x => x.ImagemAssinatura)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.CriadoEm)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.AtualizadoEm)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.DataAssinatura)
                .IsRequired(false)
                .HasColumnType("datetime2");

            builder.Property(x => x.IpAssinante)
                .IsRequired(false)
                .HasMaxLength(45);

            builder.Property(x => x.DispositivoInfo)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(x => x.Assinado)
                .HasDefaultValue(false);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(10)
                .HasConversion<string>()
                .HasDefaultValue(StatusAssinaturaEletronica.Pendente);

            builder.ToTable(t => t.HasCheckConstraint("CK_documentosVidasEletronicas_Status", 
                "[Status] IN ('Ativa', 'Vencida', 'Cancelada')"));

            builder.HasOne(x => x.Venda)
                .WithMany(x => x.DocumentosVenda)
                .HasForeignKey(x => x.VendaID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
