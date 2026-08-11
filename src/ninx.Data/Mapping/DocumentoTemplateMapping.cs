using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

namespace ninx.Data.Mappings
{
    public class DocumentoTemplateMapping : IEntityTypeConfiguration<DocumentoTemplate>
    {
        public void Configure(EntityTypeBuilder<DocumentoTemplate> builder)
        {
            builder.ToTable("DocumentosTemplate");

            builder.HasKey(x => x.TemplateID);

            builder.Property(x => x.TemplateID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.TipoDocumento)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            builder.Property(x => x.ConteudoHtml)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.Ativo)
                .HasDefaultValue(true);

            builder.Property(x => x.CriadoEm)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.AtualizadoEm)
                .IsRequired(false);

            builder.HasIndex(x => x.TipoDocumento)
                .IsUnique();
        }
    }
}
