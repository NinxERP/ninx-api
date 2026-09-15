using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AssinaturaEfetivaVendaEPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // O campo livre legado só é descartado depois de aproveitado onde o estruturado está vazio.
            migrationBuilder.Sql("""
                UPDATE Comercios
                SET EnderecoLogradouro = Endereco
                WHERE (EnderecoLogradouro IS NULL OR EnderecoLogradouro = '')
                  AND Endereco IS NOT NULL AND Endereco <> '';
                """);

            // Limite passa a ser obrigatório: clientes sem limite recebem o padrão do comércio.
            // Onde nem o comércio tem padrão, fica 0 (sem crédito) até alguém editar o cliente.
            migrationBuilder.Sql("""
                UPDATE c
                SET c.LimiteCredito = co.LimiteCreditoPadrao
                FROM Clientes c
                INNER JOIN Comercios co ON co.ComercioID = c.ComercioID
                WHERE c.LimiteCredito IS NULL AND co.LimiteCreditoPadrao IS NOT NULL;
                """);

            migrationBuilder.DropCheckConstraint(
                name: "CK_PagamentosVenda_Status",
                table: "PagamentosVenda");

            migrationBuilder.DropColumn(
                name: "Endereco",
                table: "Comercios");

            migrationBuilder.AlterColumn<decimal>(
                name: "LimiteCredito",
                table: "Clientes",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashDocumentoAssinado",
                table: "AssinaturasEletronicas",
                type: "nchar(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HashDocumentoOriginal",
                table: "AssinaturasEletronicas",
                type: "nchar(64)",
                fixedLength: true,
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PagamentoID",
                table: "AssinaturasEletronicas",
                type: "int",
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PagamentosVenda_Status",
                table: "PagamentosVenda",
                sql: "[Status] IN ('Pago', 'Estornado', 'Pendente', 'Cancelado')");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasEletronicas_PagamentoID",
                table: "AssinaturasEletronicas",
                column: "PagamentoID");

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturasEletronicas_PagamentosVenda_PagamentoID",
                table: "AssinaturasEletronicas",
                column: "PagamentoID",
                principalTable: "PagamentosVenda",
                principalColumn: "PagamentoID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturasEletronicas_PagamentosVenda_PagamentoID",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PagamentosVenda_Status",
                table: "PagamentosVenda");

            migrationBuilder.DropIndex(
                name: "IX_AssinaturasEletronicas_PagamentoID",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropColumn(
                name: "HashDocumentoAssinado",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropColumn(
                name: "HashDocumentoOriginal",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropColumn(
                name: "PagamentoID",
                table: "AssinaturasEletronicas");

            migrationBuilder.AddColumn<string>(
                name: "Endereco",
                table: "Comercios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "LimiteCredito",
                table: "Clientes",
                type: "decimal(10,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PagamentosVenda_Status",
                table: "PagamentosVenda",
                sql: "[Status] IN ('Pago', 'Estornado')");
        }
    }
}
