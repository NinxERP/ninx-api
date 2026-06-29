using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Data.Mappings
{
    public class AssinaturaMapping : IEntityTypeConfiguration<AssinaturaPlano>
    {
        public void Configure(EntityTypeBuilder<AssinaturaPlano> builder)
        {
            builder.ToTable("Assinaturas");

            builder.HasKey(x => x.AssinaturaID);

            builder.Property(x => x.AssinaturaID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Plano)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(x => x.DataInicio)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(x => x.DataFim)
                .IsRequired()
                .HasColumnType("date");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(10)
                .HasConversion<string>()
                .HasDefaultValue(StatusAssinatura.Ativa);

            builder.ToTable(t => 
            {
                t.HasCheckConstraint("CK_Assinaturas_Plano", "[Plano] IN ('Mensal', 'Trimestral', 'Anual')");
                t.HasCheckConstraint("CK_Assinaturas_Status", "[Status] IN ('Ativa', 'Vencida', 'Cancelada')");
            });

            builder.Property(x => x.CriadoEm)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.AtualizadoEm)
                .IsRequired(false);

            builder.HasOne(x => x.Comercio)
                .WithMany(x => x.Assinaturas)
                .HasForeignKey(x => x.ComercioID);
        }
    }
}