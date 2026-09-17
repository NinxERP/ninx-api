using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

namespace ninx.Data.Mappings
{
    public class PessoaAutorizadaMapping : IEntityTypeConfiguration<PessoaAutorizada>
    {
        public void Configure(EntityTypeBuilder<PessoaAutorizada> builder)
        {
            builder.ToTable("PessoasAutorizadas");

            builder.HasKey(x => x.PessoaAutorizadaID);

            builder.Property(x => x.PessoaAutorizadaID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Nome)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.Cpf)
                .IsRequired(false)
                .HasMaxLength(11);

            builder.Property(x => x.Parentesco)
                .IsRequired()
                .HasMaxLength(12)
                .HasConversion<string>();

            builder.Property(x => x.LimiteCredito)
                .IsRequired(false)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.CriadoEm)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Ignore(x => x.PodeComprar);

            builder.ToTable(t => t.HasCheckConstraint("CK_PessoasAutorizadas_Parentesco",
                "[Parentesco] IN ('Conjuge', 'Companheiro', 'Filho', 'Outro')"));

            builder.HasOne(x => x.Cliente)
                .WithMany()
                .HasForeignKey(x => x.ClienteID)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.ClienteID);
        }
    }
}
