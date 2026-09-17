using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class LimiteCreditoNoTermoEAutorizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LimitePorCompra",
                table: "PessoasAutorizadas",
                newName: "LimiteCredito");

            migrationBuilder.AddColumn<decimal>(
                name: "LimiteCredito",
                table: "TermosAberturaConta",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            // Versões já existentes concederam o limite que o cliente tem hoje.
            migrationBuilder.Sql(
                "UPDATE t SET t.LimiteCredito = c.LimiteCredito " +
                "FROM TermosAberturaConta t INNER JOIN Clientes c ON c.ClienteID = t.ClienteID");

            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "TermoAberturaConta", "ConteudoHtml", TemplateNovo);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "TermoAberturaConta", "ConteudoHtml",
                ContaFiadoTermoAberturaAutorizados.TemplateTermoAbertura);

            migrationBuilder.DropColumn(
                name: "LimiteCredito",
                table: "TermosAberturaConta");

            migrationBuilder.RenameColumn(
                name: "LimiteCredito",
                table: "PessoasAutorizadas",
                newName: "LimitePorCompra");
        }

        // O limite da pessoa autorizada deixou de ser por compra e passou a ser acumulado.
        private static readonly string TemplateNovo = ContaFiadoTermoAberturaAutorizados.TemplateTermoAbertura
            .Replace("respeitado o limite por compra", "respeitado o limite de crédito individual");
    }
}
