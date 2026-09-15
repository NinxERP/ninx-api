using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class TermoVersaoRevogacaoAssinada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Versao",
                table: "TermosAberturaConta",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevogacaoSolicitadaEm",
                table: "PessoasAutorizadas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermosAberturaConta_ClienteID_Versao",
                table: "TermosAberturaConta",
                columns: new[] { "ClienteID", "Versao" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TermosAberturaConta_ClienteID_Versao",
                table: "TermosAberturaConta");

            migrationBuilder.DropColumn(
                name: "Versao",
                table: "TermosAberturaConta");

            migrationBuilder.DropColumn(
                name: "RevogacaoSolicitadaEm",
                table: "PessoasAutorizadas");
        }
    }
}
