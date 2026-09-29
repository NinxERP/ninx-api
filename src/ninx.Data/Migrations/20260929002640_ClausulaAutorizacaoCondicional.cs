using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClausulaAutorizacaoCondicional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "TermoAberturaConta", "ConteudoHtml", TemplateNovo);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "TermoAberturaConta", "ConteudoHtml",
                LimiteCreditoNoTermoEAutorizado.TemplateNovo);
        }

        // A cláusula "Autorizo as pessoas acima relacionadas" passa a vir do código, para sumir quando
        // a lista de autorizados está vazia.
        internal static readonly string TemplateNovo = Regex.Replace(
            LimiteCreditoNoTermoEAutorizado.TemplateNovo,
            @"<div class=""declaracao"">\s*Autorizo as pessoas.*?</div>",
            "{{Html.ClausulaAutorizacao}}",
            RegexOptions.Singleline);
    }
}
