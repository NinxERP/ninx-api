using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

namespace ninx.Data.Mappings
{
    public class PagamentoAssinaturaPlanoMapping : IEntityTypeConfiguration<PagamentoAssinaturaPlano>
    {
        public void Configure(EntityTypeBuilder<PagamentoAssinaturaPlano> builder)
        {
            builder.ToTable("PagamentosAssinaturasPlano");

            builder.HasKey(x => x.PagamentoAssinaturaID);

            builder.Property(x => x.PagamentoAssinaturaID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Valor)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.DataPagamento)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.ProximoVencimento)
                .IsRequired();

            builder.Property(x => x.CriadoEm)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.AtualizadoEm)
                .IsRequired(false);

            builder.HasOne(x => x.Assinatura)
                .WithMany()
                .HasForeignKey(x => x.AssinaturaID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
