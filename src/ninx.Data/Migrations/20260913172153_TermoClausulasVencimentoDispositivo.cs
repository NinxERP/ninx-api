using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class TermoClausulasVencimentoDispositivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataVencimento",
                table: "Vendas",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaVencimentoFiado",
                table: "Comercios",
                type: "int",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AlterColumn<string>(
                name: "DispositivoInfo",
                table: "AssinaturasEletronicas",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Comercios_DiaVencimentoFiado",
                table: "Comercios",
                sql: "[DiaVencimentoFiado] BETWEEN 1 AND 31");

            AtualizarTemplates(migrationBuilder, TermoNovo, ReciboParcialNovo, ReciboGlobalNovo);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            AtualizarTemplates(migrationBuilder, Anterior.TemplateTermoCompromissoNovo,
                Anterior.TemplateReciboPagamentoParcialNovo, Anterior.TemplateReciboQuitacaoGlobalNovo);

            migrationBuilder.DropCheckConstraint(
                name: "CK_Comercios_DiaVencimentoFiado",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "DataVencimento",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "DiaVencimentoFiado",
                table: "Comercios");

            migrationBuilder.AlterColumn<string>(
                name: "DispositivoInfo",
                table: "AssinaturasEletronicas",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        private static void AtualizarTemplates(MigrationBuilder migrationBuilder, string termo, string reciboParcial, string reciboGlobal)
        {
            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "TermoCompromisso", "ConteudoHtml", termo);
            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "ReciboPagamentoParcial", "ConteudoHtml", reciboParcial);
            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "ReciboQuitacaoGlobal", "ConteudoHtml", reciboGlobal);
        }

        internal sealed class Anterior
        {
            internal const string TemplateTermoCompromissoNovo = AjustaLayoutAssinaturaCredorTemplates.TemplateTermoCompromissoNovo;
            internal const string TemplateReciboPagamentoParcialNovo = AjustaLayoutAssinaturaCredorTemplates.TemplateReciboPagamentoParcialNovo;
            internal const string TemplateReciboQuitacaoGlobalNovo = AjustaLayoutAssinaturaCredorTemplates.TemplateReciboQuitacaoGlobalNovo;
        }

        // Aceite da assinatura eletrônica: é o "admitido pelas partes como válido" do art. 10, § 2º, da MP 2.200-2/2001.
        private const string ClausulaAssinaturaEletronica = """
              <div class="declaracao">
                As partes reconhecem como válida a assinatura eletrônica aposta neste documento, bem como os registros de
                data, hora, endereço IP e dispositivo que a acompanham, nos termos do art. 10, § 2º, da Medida Provisória
                nº 2.200-2/2001.
              </div>
            """;

        internal const string ClausulaReconhecimentoDivida = """
              <div class="declaracao">
                Declaro ter recebido os produtos acima descritos e reconheço o saldo devedor de
                <strong>{{Venda.SaldoDevedor}}</strong>, que me comprometo a pagar a {{Comercio.Nome}} até
                <strong>{{Venda.DataVencimento}}</strong>.
              </div>
            """;

        private const string AntesDasAssinaturas = "  <table class=\"assinaturas\">";

        private const string LinhaSaldoTermo = "    <tr class=\"destaque\"><td>Saldo Devedor Remanescente</td><td class=\"valor\">{{Venda.SaldoDevedor}}</td></tr>";

        private const string LinhaVencimento = "\n    <tr><td class=\"label\">Vencimento</td><td class=\"valor\">{{Venda.DataVencimento}}</td></tr>";

        internal static readonly string TermoNovo = Anterior.TemplateTermoCompromissoNovo
            .Replace(LinhaSaldoTermo, LinhaSaldoTermo + LinhaVencimento)
            .Replace(AntesDasAssinaturas, ClausulaReconhecimentoDivida + "\n\n" + ClausulaAssinaturaEletronica + "\n\n" + AntesDasAssinaturas);

        private static readonly string ReciboParcialNovo = Anterior.TemplateReciboPagamentoParcialNovo
            .Replace(AntesDasAssinaturas, ClausulaAssinaturaEletronica + "\n\n" + AntesDasAssinaturas);

        private static readonly string ReciboGlobalNovo = Anterior.TemplateReciboQuitacaoGlobalNovo
            .Replace(AntesDasAssinaturas, ClausulaAssinaturaEletronica + "\n\n" + AntesDasAssinaturas);
    }
}
