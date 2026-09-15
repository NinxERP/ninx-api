using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Data.Mappings
{
    public class AssinaturaEletronicaMapping : IEntityTypeConfiguration<AssinaturaEletronica>
    {
        public void Configure(EntityTypeBuilder<AssinaturaEletronica> builder)
        {
            builder.ToTable("AssinaturasEletronicas");

            builder.HasKey(x => x.AssinaturaID);

            builder.Property(x => x.AssinaturaID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.DocumentoGuid)
                .IsRequired();

            builder.Property(x => x.TipoDocumento)
                .IsRequired(false)
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.DocumentoOriginalBase64)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.DocumentoHtmlMesclado)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.DocumentoAssinadoBase64)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.ImagemAssinatura)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            // SHA-256 em hexadecimal: sempre 64 caracteres.
            builder.Property(x => x.HashDocumentoOriginal)
                .IsRequired(false)
                .HasMaxLength(64)
                .IsFixedLength();

            builder.Property(x => x.HashDocumentoAssinado)
                .IsRequired(false)
                .HasMaxLength(64)
                .IsFixedLength();

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

            // Agente de usuário de navegador móvel pode passar de 200 caracteres.
            builder.Property(x => x.DispositivoInfo)
                .IsRequired(false)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Assinado)
                .HasDefaultValue(false);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(10)
                .HasConversion<string>()
                .HasDefaultValue(StatusAssinatura.Ativa);

            builder.ToTable(t => t.HasCheckConstraint("CK_AssinaturasEletronicas_Status", 
                "[Status] IN ('Ativa', 'Vencida', 'Cancelada')"));

            builder.HasOne(x => x.Venda)
                .WithMany(x => x.AssinaturasEletronicas)
                .HasForeignKey(x => x.VendaID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TermoAbertura)
                .WithMany()
                .HasForeignKey(x => x.TermoAberturaID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Todo documento pertence a exatamente um dono: uma venda ou um termo de abertura.
            builder.ToTable(t => t.HasCheckConstraint("CK_AssinaturasEletronicas_Dono",
                "([VendaID] IS NOT NULL AND [TermoAberturaID] IS NULL) OR ([VendaID] IS NULL AND [TermoAberturaID] IS NOT NULL)"));

            builder.HasOne(x => x.Pagamento)
                .WithMany()
                .HasForeignKey(x => x.PagamentoID)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
