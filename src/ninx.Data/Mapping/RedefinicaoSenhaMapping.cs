using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class RedefinicaoSenhaMapping : IEntityTypeConfiguration<RedefinicaoSenha>
{
    public void Configure(EntityTypeBuilder<RedefinicaoSenha> builder)
    {
        builder.ToTable("RedefinicoesSenha");

        builder.HasKey(x => x.RedefinicaoSenhaID);

        builder.Property(x => x.RedefinicaoSenhaID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.CodigoHash)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioID);

        builder.HasIndex(x => new { x.UsuarioID, x.Utilizado, x.ExpiraEm });
    }
}
