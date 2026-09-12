using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubstituiPesoCargoPorPermissoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EhProprietario",
                table: "Cargos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Permissoes",
                columns: table => new
                {
                    PermissaoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Chave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissoes", x => x.PermissaoID);
                });

            migrationBuilder.CreateTable(
                name: "CargosPermissoes",
                columns: table => new
                {
                    CargoPermissaoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CargoID = table.Column<int>(type: "int", nullable: false),
                    PermissaoID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CargosPermissoes", x => x.CargoPermissaoID);
                    table.ForeignKey(
                        name: "FK_CargosPermissoes_Cargos_CargoID",
                        column: x => x.CargoID,
                        principalTable: "Cargos",
                        principalColumn: "CargoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CargosPermissoes_Permissoes_PermissaoID",
                        column: x => x.PermissaoID,
                        principalTable: "Permissoes",
                        principalColumn: "PermissaoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CargosPermissoes_CargoID_PermissaoID",
                table: "CargosPermissoes",
                columns: new[] { "CargoID", "PermissaoID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CargosPermissoes_PermissaoID",
                table: "CargosPermissoes",
                column: "PermissaoID");

            migrationBuilder.CreateIndex(
                name: "IX_Permissoes_Chave",
                table: "Permissoes",
                column: "Chave",
                unique: true);

            // Permissões disponíveis para compor cargos (ver PermissaoConstantes).
            migrationBuilder.Sql(
                "INSERT INTO Permissoes (Chave, Nome, Descricao) VALUES " +
                "('GerenciarUsuarios', 'Gerenciar usuários', 'Criar, editar, desativar e vincular usuários do comércio'), " +
                "('GerenciarCargos', 'Gerenciar cargos', 'Criar, editar e desativar cargos do comércio'), " +
                "('GerenciarComercio', 'Gerenciar comércio', 'Editar os dados cadastrais do comércio'), " +
                "('GerenciarAssinatura', 'Gerenciar assinatura', 'Consultar e cancelar a assinatura do plano'), " +
                "('GerenciarPagamentos', 'Gerenciar pagamentos', 'Consultar o histórico de pagamentos da assinatura'), " +
                "('VisualizarRelatorios', 'Visualizar relatórios', 'Consultar relatórios e comparativos entre comércios')");

            // Migração de dados: cargos que antes tinham Peso >= 20 (Dono/Admin, topo da hierarquia) viram
            // "proprietário" e recebem todas as permissões; os demais (ex.: Funcionario) ficam sem nenhuma
            // permissão por padrão — o dono do comércio revisa e concede depois pela tela de gestão de cargos.
            migrationBuilder.Sql("UPDATE Cargos SET EhProprietario = 1 WHERE Peso >= 20");

            migrationBuilder.Sql(
                "INSERT INTO CargosPermissoes (CargoID, PermissaoID) " +
                "SELECT c.CargoID, p.PermissaoID FROM Cargos c CROSS JOIN Permissoes p WHERE c.Peso >= 20");

            migrationBuilder.DropColumn(
                name: "Peso",
                table: "Cargos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversão: reconstrói Peso a partir de EhProprietario (20 para proprietário, 10 caso contrário).
            // Os pesos originais (para eventuais cargos customizados com peso diferente de 20/10) não são
            // recuperáveis, já que a granularidade foi substituída por permissões nesta migração.
            migrationBuilder.AddColumn<int>(
                name: "Peso",
                table: "Cargos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE Cargos SET Peso = CASE WHEN EhProprietario = 1 THEN 20 ELSE 10 END");

            migrationBuilder.DropTable(
                name: "CargosPermissoes");

            migrationBuilder.DropTable(
                name: "Permissoes");

            migrationBuilder.DropColumn(
                name: "EhProprietario",
                table: "Cargos");
        }
    }
}
