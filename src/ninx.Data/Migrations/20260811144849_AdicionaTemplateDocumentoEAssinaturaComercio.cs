using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTemplateDocumentoEAssinaturaComercio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssinaturaResponsavelBase64",
                table: "Comercios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoAssinadoBase64",
                table: "AssinaturasEletronicas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoHtmlMesclado",
                table: "AssinaturasEletronicas",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                table: "AssinaturasEletronicas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DocumentosTemplate",
                columns: table => new
                {
                    TemplateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoDocumento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ConteudoHtml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosTemplate", x => x.TemplateID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosTemplate_TipoDocumento",
                table: "DocumentosTemplate",
                column: "TipoDocumento",
                unique: true);

            migrationBuilder.InsertData(
                table: "DocumentosTemplate",
                columns: new[] { "TipoDocumento", "ConteudoHtml", "Ativo" },
                values: new object[,]
                {
                    { "TermoCompromisso", TemplateTermoCompromisso, true },
                    { "ReciboPagamentoParcial", TemplateReciboPagamentoParcial, true },
                    { "ReciboQuitacaoGlobal", TemplateReciboQuitacaoGlobal, true }
                });
        }

        private const string EstiloComum = """
            @page { size: A4; margin: 35px 45px; }
            body { font-family: Helvetica, Arial, sans-serif; color: #1E293B; font-size: 10pt; }
            h1 { font-size: 20pt; color: #0D1B2A; margin: 0 0 4px 0; }
            .subtitulo { font-size: 10pt; color: #0EA5E9; font-weight: bold; letter-spacing: 1px; margin-bottom: 25px; }
            .envolvidos { width: 100%; border-collapse: separate; border-spacing: 12px 0; margin-bottom: 10px; }
            .card { background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 6px; padding: 12px; width: 50%; vertical-align: top; font-size: 10pt; }
            .card-titulo { font-size: 9.5pt; color: #64748B; font-weight: bold; margin-bottom: 6px; }
            .secao-titulo { font-size: 11pt; color: #0D1B2A; font-weight: bold; margin-top: 20px; margin-bottom: 8px; }
            .resumo { background: #F8FAFC; border: 1px solid #E2E8F0; border-radius: 6px; padding: 14px; margin-top: 8px; }
            .resumo table { width: 100%; }
            .resumo td { padding: 4px 0; font-size: 10pt; }
            .resumo .label { color: #64748B; }
            .resumo .valor { text-align: right; }
            .resumo .destaque td { font-weight: bold; font-size: 11.5pt; border-top: 1px dashed #E2E8F0; padding-top: 8px; }
            .declaracao { font-size: 10.5pt; margin: 20px 0; line-height: 1.4; }
            .assinaturas { width: 100%; margin-top: 45px; }
            .assinaturas .linha-imagem td { width: 50%; height: 40px; vertical-align: bottom; text-align: center; padding: 0 20px; }
            .assinaturas .linha-dados td { width: 50%; text-align: center; padding: 10px 20px 0 20px; border-top: 0.75px solid #64748B; font-size: 9.5pt; }
            .assinaturas .rotulo { display: block; font-size: 8.5pt; color: #64748B; font-weight: bold; margin-bottom: 4px; }
            .rodape { text-align: center; color: #64748B; font-size: 8.5pt; margin-top: 40px; }
            .bloco-assinatura { margin-top: 8px; }
            """;

        private const string TemplateTermoCompromisso = """
            <!DOCTYPE html>
            <html><head><meta charset="utf-8" /><style>
            """ + EstiloComum + """
            </style></head>
            <body>
              <h1>TERMO DE COMPROMISSO</h1>
              <div class="subtitulo">PAGAMENTO E VENDA A PRAZO</div>

              <table class="envolvidos"><tr>
                <td class="card">
                  <div class="card-titulo">CREDOR (EMPRESA)</div>
                  <div>Razão Social: {{Comercio.Nome}}</div>
                  <div>CNPJ: {{Comercio.Cnpj}}</div>
                  <div>Endereço: {{Comercio.Endereco}}</div>
                </td>
                <td class="card">
                  <div class="card-titulo">DEVEDOR (CLIENTE)</div>
                  <div>Nome: {{Cliente.Nome}}</div>
                  <div>CPF: {{Cliente.Cpf}}</div>
                  <div>Endereço: {{Cliente.Endereco}}</div>
                  <div>Telefone: {{Cliente.Telefone}}</div>
                </td>
              </tr></table>

              <div class="secao-titulo">ITENS DA VENDA</div>
              {{Html.TabelaItens}}

              <div class="secao-titulo">RESUMO FINANCEIRO E CONDIÇÕES</div>
              <div class="resumo"><table>
                <tr><td class="label">Valor Total da Venda</td><td class="valor">{{Venda.Total}}</td></tr>
                <tr><td class="label">Valor Pago de Entrada</td><td class="valor">{{Venda.ValorPago}}</td></tr>
                <tr class="destaque"><td>Saldo Devedor Remanescente</td><td class="valor">{{Venda.SaldoDevedor}}</td></tr>
              </table></div>

              <table class="assinaturas">
                <tr class="linha-imagem">
                  <td></td>
                  <td>{{Html.ComercioAssinatura}}</td>
                </tr>
                <tr class="linha-dados">
                  <td><span class="rotulo">ASSINATURA DO DEVEDOR</span>{{Cliente.Nome}}<div class="bloco-assinatura">{{Html.BlocoAssinatura}}</div></td>
                  <td><span class="rotulo">ASSINATURA DO CREDOR</span>{{Comercio.Nome}}</td>
                </tr>
              </table>

              <div class="rodape">Data de Emissão: {{Data}}</div>
            </body></html>
            """;

        private const string TemplateReciboPagamentoParcial = """
            <!DOCTYPE html>
            <html><head><meta charset="utf-8" /><style>
            """ + EstiloComum + """
            </style></head>
            <body>
              <h1>RECIBO DE PAGAMENTO PARCIAL</h1>
              <div class="subtitulo">REFERENTE À VENDA #{{Venda.VendaID}}</div>

              <table class="envolvidos"><tr>
                <td class="card">
                  <div class="card-titulo">CREDOR</div>
                  <div>{{Comercio.Nome}}</div>
                  <div>CNPJ: {{Comercio.Cnpj}}</div>
                </td>
                <td class="card">
                  <div class="card-titulo">DEVEDOR</div>
                  <div>{{Cliente.Nome}}</div>
                  <div>CPF: {{Cliente.Cpf}}</div>
                  <div>Endereço: {{Cliente.Endereco}}</div>
                  <div>Telefone: {{Cliente.Telefone}}</div>
                </td>
              </tr></table>

              <div class="declaracao">
                Declaramos para os devidos fins que o devedor acima identificado realizou o pagamento manual da quantia de
                <strong>{{Pagamento.Valor}}</strong> através da forma de pagamento <strong>{{Pagamento.FormaPagamento}}</strong>,
                abatendo do saldo devedor remanescente desta transação comercial.
              </div>

              <div class="secao-titulo">DEMONSTRATIVO DO SALDO</div>
              <div class="resumo"><table>
                <tr><td class="label">Saldo Devedor Antes deste Pagamento</td><td class="valor">{{Venda.SaldoAnterior}}</td></tr>
                <tr><td class="label">Valor Pago Neste Ato (-)</td><td class="valor">{{Pagamento.Valor}}</td></tr>
                <tr class="destaque"><td>Saldo Devedor Atual Restante</td><td class="valor">{{Venda.SaldoNovo}}</td></tr>
              </table></div>

              <table class="assinaturas">
                <tr class="linha-imagem">
                  <td></td>
                  <td>{{Html.ComercioAssinatura}}</td>
                </tr>
                <tr class="linha-dados">
                  <td><span class="rotulo">ASSINATURA DO CLIENTE</span>{{Cliente.Nome}}<div class="bloco-assinatura">{{Html.BlocoAssinatura}}</div></td>
                  <td><span class="rotulo">RESPONSÁVEL RECEBIMENTO</span>{{Comercio.Nome}}</td>
                </tr>
              </table>

              <div class="rodape">Recibo emitido em: {{Data}}</div>
            </body></html>
            """;

        private const string TemplateReciboQuitacaoGlobal = """
            <!DOCTYPE html>
            <html><head><meta charset="utf-8" /><style>
            """ + EstiloComum + """
            </style></head>
            <body>
              <h1>RECIBO DE QUITAÇÃO GLOBAL / ABATIMENTO</h1>
              <div class="subtitulo">CONTA FIADO - MÚLTIPLAS TRANSAÇÕES</div>

              <table class="envolvidos"><tr>
                <td class="card">
                  <div class="card-titulo">CREDOR</div>
                  <div>{{Comercio.Nome}}</div>
                </td>
                <td class="card">
                  <div class="card-titulo">DEVEDOR</div>
                  <div>{{Cliente.Nome}}</div>
                  <div>CPF: {{Cliente.Cpf}}</div>
                  <div>Endereço: {{Cliente.Endereco}}</div>
                </td>
              </tr></table>

              <div class="declaracao">
                Confirmamos o recebimento do valor total de <strong>{{ValorTotal}}</strong> via de pagamento <strong>{{FormaPagamento}}</strong>,
                utilizado para abater e/ou quitar o saldo devedor das vendas descritas no demonstrativo abaixo:
              </div>

              {{Html.TabelaDistribuicao}}

              <table class="assinaturas">
                <tr class="linha-imagem">
                  <td></td>
                  <td>{{Html.ComercioAssinatura}}</td>
                </tr>
                <tr class="linha-dados">
                  <td><span class="rotulo">ASSINATURA DO CLIENTE</span>{{Cliente.Nome}}<div class="bloco-assinatura">{{Html.BlocoAssinatura}}</div></td>
                  <td><span class="rotulo">RESPONSÁVEL RECEBIMENTO</span>{{Comercio.Nome}}</td>
                </tr>
              </table>

              <div class="rodape">Documento Global emitido em: {{Data}}</div>
            </body></html>
            """;

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentosTemplate");

            migrationBuilder.DropColumn(
                name: "AssinaturaResponsavelBase64",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "DocumentoAssinadoBase64",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropColumn(
                name: "DocumentoHtmlMesclado",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "AssinaturasEletronicas");
        }
    }
}
