using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

public class PagamentoVendaMapping : IEntityTypeConfiguration<PagamentoVenda>
{
    public void Configure(EntityTypeBuilder<PagamentoVenda> builder)
    {
        builder.ToTable("PagamentosVenda");

        builder.HasKey(x => x.PagamentoID);

        builder.Property(x => x.PagamentoID)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.FormaPagamento)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Valor)
            .IsRequired()
            .HasColumnType("decimal(10,2)");

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(10)
            .HasConversion<string>()
            .HasDefaultValue(StatusPagamento.Pago);

        builder.ToTable(t => 
        {
            t.HasCheckConstraint("CK_PagamentosVenda_FormaPagamento", "[FormaPagamento] IN ('Dinheiro', 'Pix', 'Cartao')");
            t.HasCheckConstraint("CK_PagamentosVenda_Status", "[Status] IN ('Pago', 'Estornado')");
        });

        builder.Property(x => x.CriadoEm)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.AtualizadoEm)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.Venda)
            .WithMany(x => x.PagamentosVenda)
            .HasForeignKey(x => x.VendaID)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}