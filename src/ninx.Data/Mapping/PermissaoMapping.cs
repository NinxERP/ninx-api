using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class PermissaoMapping : IEntityTypeConfiguration<Permissao>
{
    public void Configure(EntityTypeBuilder<Permissao> builder)
    {
        builder.ToTable("Permissoes");

        builder.HasKey(x => x.PermissaoID);

        builder.Property(x => x.PermissaoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Chave)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Descricao)
            .HasMaxLength(255)
            .IsRequired(false);

        builder.HasIndex(x => x.Chave)
            .IsUnique();
    }
}
