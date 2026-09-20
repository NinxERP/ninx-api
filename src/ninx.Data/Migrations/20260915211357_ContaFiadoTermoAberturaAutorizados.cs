using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class ContaFiadoTermoAberturaAutorizados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PessoaAutorizadaID",
                table: "Vendas",
                type: "int",
                nullable: true);

            // O SQL Server não altera coluna usada por índice ou chave estrangeira: remove e recria.
            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturasEletronicas_Vendas_VendaID",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropIndex(
                name: "IX_AssinaturasEletronicas_VendaID",
                table: "AssinaturasEletronicas");

            migrationBuilder.AlterColumn<int>(
                name: "VendaID",
                table: "AssinaturasEletronicas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasEletronicas_VendaID",
                table: "AssinaturasEletronicas",
                column: "VendaID");

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturasEletronicas_Vendas_VendaID",
                table: "AssinaturasEletronicas",
                column: "VendaID",
                principalTable: "Vendas",
                principalColumn: "VendaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<int>(
                name: "TermoAberturaID",
                table: "AssinaturasEletronicas",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PessoasAutorizadas",
                columns: table => new
                {
                    PessoaAutorizadaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteID = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cpf = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    Parentesco = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    MenorDeIdade = table.Column<bool>(type: "bit", nullable: false),
                    LimitePorCompra = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AutorizadaEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevogadaEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PessoasAutorizadas", x => x.PessoaAutorizadaID);
                    table.CheckConstraint("CK_PessoasAutorizadas_Parentesco", "[Parentesco] IN ('Conjuge', 'Companheiro', 'Filho', 'Outro')");
                    table.ForeignKey(
                        name: "FK_PessoasAutorizadas_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "ClienteID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TermosAberturaConta",
                columns: table => new
                {
                    TermoAberturaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AssinadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermosAberturaConta", x => x.TermoAberturaID);
                    table.CheckConstraint("CK_TermosAberturaConta_Status", "[Status] IN ('Aguardando', 'Ativo', 'Substituido', 'Cancelado')");
                    table.ForeignKey(
                        name: "FK_TermosAberturaConta_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "ClienteID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_PessoaAutorizadaID",
                table: "Vendas",
                column: "PessoaAutorizadaID");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasEletronicas_TermoAberturaID",
                table: "AssinaturasEletronicas",
                column: "TermoAberturaID");

            migrationBuilder.AddCheckConstraint(
                name: "CK_AssinaturasEletronicas_Dono",
                table: "AssinaturasEletronicas",
                sql: "([VendaID] IS NOT NULL AND [TermoAberturaID] IS NULL) OR ([VendaID] IS NULL AND [TermoAberturaID] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_PessoasAutorizadas_ClienteID",
                table: "PessoasAutorizadas",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_TermosAberturaConta_ClienteID_Status",
                table: "TermosAberturaConta",
                columns: new[] { "ClienteID", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturasEletronicas_TermosAberturaConta_TermoAberturaID",
                table: "AssinaturasEletronicas",
                column: "TermoAberturaID",
                principalTable: "TermosAberturaConta",
                principalColumn: "TermoAberturaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendas_PessoasAutorizadas_PessoaAutorizadaID",
                table: "Vendas",
                column: "PessoaAutorizadaID",
                principalTable: "PessoasAutorizadas",
                principalColumn: "PessoaAutorizadaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "TermoCompromisso", "ConteudoHtml", TermoCompromissoNovo);
            migrationBuilder.InsertData("DocumentosTemplate",
                new[] { "TipoDocumento", "ConteudoHtml", "Ativo" },
                new object[] { "TermoAberturaConta", TemplateTermoAbertura, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("DocumentosTemplate", "TipoDocumento", "TermoAberturaConta");
            migrationBuilder.UpdateData("DocumentosTemplate", "TipoDocumento", "TermoCompromisso", "ConteudoHtml",
                TermoClausulasVencimentoDispositivo.TermoNovo);

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturasEletronicas_TermosAberturaConta_TermoAberturaID",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropForeignKey(
                name: "FK_Vendas_PessoasAutorizadas_PessoaAutorizadaID",
                table: "Vendas");

            migrationBuilder.DropTable(
                name: "PessoasAutorizadas");

            migrationBuilder.DropTable(
                name: "TermosAberturaConta");

            migrationBuilder.DropIndex(
                name: "IX_Vendas_PessoaAutorizadaID",
                table: "Vendas");

            migrationBuilder.DropIndex(
                name: "IX_AssinaturasEletronicas_TermoAberturaID",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_AssinaturasEletronicas_Dono",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropColumn(
                name: "PessoaAutorizadaID",
                table: "Vendas");

            migrationBuilder.DropColumn(
                name: "TermoAberturaID",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropForeignKey(
                name: "FK_AssinaturasEletronicas_Vendas_VendaID",
                table: "AssinaturasEletronicas");

            migrationBuilder.DropIndex(
                name: "IX_AssinaturasEletronicas_VendaID",
                table: "AssinaturasEletronicas");

            migrationBuilder.AlterColumn<int>(
                name: "VendaID",
                table: "AssinaturasEletronicas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasEletronicas_VendaID",
                table: "AssinaturasEletronicas",
                column: "VendaID");

            migrationBuilder.AddForeignKey(
                name: "FK_AssinaturasEletronicas_Vendas_VendaID",
                table: "AssinaturasEletronicas",
                column: "VendaID",
                principalTable: "Vendas",
                principalColumn: "VendaID",
                onDelete: ReferentialAction.Restrict);
        }

        // A cláusula de reconhecimento e o nome de quem assina passam a vir do código, porque mudam
        // quando a compra é feita por uma pessoa autorizada em nome do titular.
        private static readonly string TermoCompromissoNovo = TermoClausulasVencimentoDispositivo.TermoNovo
            .Replace(TermoClausulasVencimentoDispositivo.ClausulaReconhecimentoDivida, "  {{Html.ClausulaReconhecimento}}")
            .Replace("<span class=\"rotulo\">ASSINATURA DO DEVEDOR</span>{{Cliente.Nome}}",
                     "<span class=\"rotulo\">{{Assinatura.Rotulo}}</span>{{Assinatura.Nome}}");

        // PENDENTE (plano-evolucao-fiado.md, fase 1): redação das cláusulas a revisar com advogado.
        internal const string TemplateTermoAbertura = """
            <!DOCTYPE html>
            <html><head><meta charset="utf-8" /><style>
            """ + AjustaLayoutAssinaturaCredorTemplates.EstiloComumNovo + """
            </style></head>
            <body>
              <h1>TERMO DE ABERTURA DE CONTA</h1>
              <div class="subtitulo">COMPRAS A PRAZO E PESSOAS AUTORIZADAS · VERSÃO {{Termo.Versao}}</div>

              <table class="envolvidos"><tr>
                <td class="card">
                  <div class="card-titulo">CREDOR (EMPRESA)</div>
                  <div>Razão Social: {{Comercio.Nome}}</div>
                  <div>CNPJ: {{Comercio.Cnpj}}</div>
                  <div>Endereço: {{Comercio.Endereco}}</div>
                </td>
                <td class="card">
                  <div class="card-titulo">TITULAR DA CONTA</div>
                  <div>Nome: {{Cliente.Nome}}</div>
                  <div>CPF: {{Cliente.Cpf}}</div>
                  <div>Endereço: {{Cliente.Endereco}}</div>
                  <div>Telefone: {{Cliente.Telefone}}</div>
                </td>
              </tr></table>

              <div class="secao-titulo">CONDIÇÕES DA CONTA</div>
              <div class="resumo"><table>
                <tr><td class="label">Limite de crédito</td><td class="valor">{{Cliente.LimiteCredito}}</td></tr>
                <tr><td class="label">Vencimento das compras</td><td class="valor">Dia {{Comercio.DiaVencimento}} de cada mês</td></tr>
              </table></div>

              <div class="declaracao">
                Declaro que as compras a prazo registradas em minha conta neste estabelecimento, realizadas por mim ou pelas
                pessoas autorizadas relacionadas abaixo, constituem dívida de minha responsabilidade, a ser paga no vencimento
                indicado no documento de cada compra.
              </div>

              <div class="secao-titulo">PESSOAS AUTORIZADAS</div>
              {{Html.TabelaAutorizados}}

              <div class="declaracao">
                Autorizo as pessoas acima relacionadas a realizar compras a prazo em minha conta, respeitado o limite por compra
                indicado. Esta versão substitui as anteriores a partir de sua assinatura; a inclusão ou a revogação de qualquer
                autorização será feita por nova versão deste termo, assinada por mim.
              </div>

              {{Html.Revogacoes}}

              <div class="declaracao">
                As partes reconhecem como válida a assinatura eletrônica aposta neste documento, bem como os registros de
                data, hora, endereço IP e dispositivo que a acompanham, nos termos do art. 10, § 2º, da Medida Provisória
                nº 2.200-2/2001.
              </div>

              <table class="assinaturas">
                <tr class="linha-imagem">
                  <td></td>
                  <td>{{Html.ComercioAssinatura}}</td>
                </tr>
                <tr class="linha-dados">
                  <td><span class="rotulo">ASSINATURA DO TITULAR</span>{{Cliente.Nome}}<div class="bloco-assinatura">{{Html.BlocoAssinatura}}</div></td>
                  <td><span class="rotulo">ASSINATURA DO CREDOR</span>{{Comercio.Nome}}</td>
                </tr>
              </table>

              <div class="rodape">Data de Emissão: {{Data}}</div>
            </body></html>
            """;
    }
}
