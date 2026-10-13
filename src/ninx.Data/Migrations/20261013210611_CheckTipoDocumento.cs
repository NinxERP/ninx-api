using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class CheckTipoDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_DocumentosTemplate_TipoDocumento",
                table: "DocumentosTemplate",
                sql: "[TipoDocumento] IN ('TermoCompromisso', 'ReciboPagamentoParcial', 'ReciboQuitacaoGlobal', 'TermoAberturaConta')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AssinaturasEletronicas_TipoDocumento",
                table: "AssinaturasEletronicas",
                sql: "[TipoDocumento] IS NULL OR [TipoDocumento] IN ('TermoCompromisso', 'ReciboPagamentoParcial', 'ReciboQuitacaoGlobal', 'TermoAberturaConta')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_DocumentosTemplate_TipoDocumento",
                table: "DocumentosTemplate");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AssinaturasEletronicas_TipoDocumento",
                table: "AssinaturasEletronicas");
        }
    }
}
