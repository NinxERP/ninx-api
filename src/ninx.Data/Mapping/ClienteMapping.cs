using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

namespace ninx.Data.Mappings
{
    public class ClienteMapping : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.ToTable("Clientes");

            builder.HasKey(x => x.ClienteID);

            builder.Property(x => x.ClienteID)
                .ValueGeneratedOnAdd();

            builder.Property(x => x.Nome)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Telefone)
                .IsRequired(false)
                .HasMaxLength(20);

            builder.Property(x => x.LimiteCredito)
                .IsRequired(false)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.Ativo)
                .HasDefaultValue(true);

            builder.Property(x => x.CriadoEm)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.AtualizadoEm)
                .IsRequired(false);

            builder.HasOne(x => x.Comercio)
                .WithMany(x => x.Clientes)
                .HasForeignKey(x => x.ComercioID);

        }
    }
}