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

            builder.Property(x => x.Cpf)
                .IsRequired()
                .HasMaxLength(11)
                .HasDefaultValue(string.Empty);

            builder.Property(x => x.Email)
                .IsRequired(false)
                .HasMaxLength(150);

            builder.Property(x => x.EnderecoLogradouro)
                .IsRequired()
                .HasMaxLength(200)
                .HasDefaultValue(string.Empty);

            builder.Property(x => x.EnderecoNumero)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue(string.Empty);

            builder.Property(x => x.EnderecoComplemento)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(x => x.EnderecoBairro)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue(string.Empty);

            builder.Property(x => x.EnderecoCidade)
                .IsRequired()
                .HasMaxLength(100)
                .HasDefaultValue(string.Empty);

            builder.Property(x => x.EnderecoUF)
                .IsRequired()
                .HasMaxLength(2)
                .HasDefaultValue(string.Empty);

            builder.Property(x => x.EnderecoCEP)
                .IsRequired()
                .HasMaxLength(9)
                .HasDefaultValue(string.Empty);

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

            builder.HasIndex(x => new { x.ComercioID, x.Cpf })
                .IsUnique()
                .HasFilter("[Cpf] <> ''");

        }
    }
}