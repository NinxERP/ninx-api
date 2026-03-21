using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;

public class MovimentacaoEstoqueMapping : IEntityTypeConfiguration<MovimentacaoEstoque>
{
    public void Configure(EntityTypeBuilder<MovimentacaoEstoque> builder)
    {
        builder.ToTable("MovimentacoesEstoque");

        builder.HasKey(x => x.MovimentacaoID);

        builder.Property(x => x.MovimentacaoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion<string>();

        builder.Property(x => x.Quantidade)
            .IsRequired()
            .HasColumnType("decimal(10,3)");

        builder.Property(x => x.ReferenciaID)
            .IsRequired(false);

        builder.Property(x => x.Observacao)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(x => x.DataHora)
            .HasDefaultValueSql("GETDATE()");

        builder.HasOne(x => x.Comercio)
            .WithMany()
            .HasForeignKey(x => x.ComercioID);

        builder.HasOne(x => x.Produto)
            .WithMany()
            .HasForeignKey(x => x.ProdutoID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}