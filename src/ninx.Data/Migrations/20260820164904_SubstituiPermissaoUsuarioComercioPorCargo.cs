using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubstituiPermissaoUsuarioComercioPorCargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cargos",
                columns: table => new
                {
                    CargoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Peso = table.Column<int>(type: "int", nullable: false),
                    ComercioID = table.Column<int>(type: "int", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cargos", x => x.CargoID);
                    table.ForeignKey(
                        name: "FK_Cargos_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_ComercioID_Nome",
                table: "Cargos",
                columns: new[] { "ComercioID", "Nome" },
                unique: true,
                filter: "[ComercioID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Cargos_Nome",
                table: "Cargos",
                column: "Nome",
                unique: true,
                filter: "[ComercioID] IS NULL");

            // Cargos base: só Dono e Funcionario. O cargo "Administrador" deixa de existir como cargo de
            // comércio — operações antes exclusivas dele agora checam Usuario.Admin (ver AutorizacaoGlobalService).
            // Peso espaçado de 10 em 10 para permitir cargos customizados intermediários no futuro.
            migrationBuilder.Sql(
                "INSERT INTO Cargos (Nome, Peso, ComercioID, Ativo, CriadoEm) VALUES " +
                "('Dono', 20, NULL, 1, GETUTCDATE()), " +
                "('Funcionario', 10, NULL, 1, GETUTCDATE())");

            migrationBuilder.AddColumn<int>(
                name: "CargoID",
                table: "UsuariosComercios",
                type: "int",
                nullable: true);

            // Backfill: Administrador vira Dono (era o topo da hierarquia de um comércio, papel que agora é do Dono).
            migrationBuilder.Sql(
                "UPDATE uc SET uc.CargoID = c.CargoID " +
                "FROM UsuariosComercios uc " +
                "JOIN Cargos c ON c.ComercioID IS NULL " +
                "  AND c.Nome = CASE WHEN uc.Permissao = 'Administrador' THEN 'Dono' ELSE uc.Permissao END");

            migrationBuilder.AlterColumn<int>(
                name: "CargoID",
                table: "UsuariosComercios",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.DropCheckConstraint(
                name: "CK_UsuariosComercios_Permissao",
                table: "UsuariosComercios");

            migrationBuilder.DropColumn(
                name: "Permissao",
                table: "UsuariosComercios");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosComercios_CargoID",
                table: "UsuariosComercios",
                column: "CargoID");

            migrationBuilder.AddForeignKey(
                name: "FK_UsuariosComercios_Cargos_CargoID",
                table: "UsuariosComercios",
                column: "CargoID",
                principalTable: "Cargos",
                principalColumn: "CargoID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UsuariosComercios_Cargos_CargoID",
                table: "UsuariosComercios");

            migrationBuilder.DropIndex(
                name: "IX_UsuariosComercios_CargoID",
                table: "UsuariosComercios");

            migrationBuilder.AddColumn<string>(
                name: "Permissao",
                table: "UsuariosComercios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // Reversão: só reconstrói corretamente se nenhum vínculo estiver usando um cargo customizado
            // (fora de 'Dono'/'Funcionario') — a granularidade original de 'Administrador' também não é
            // recuperável, pois foi unificada em 'Dono' na migração de ida. Vínculos com cargo fora desses
            // dois nomes ficarão com Permissao NULL e vão violar o CHECK CONSTRAINT abaixo; nesse cenário
            // a reversão precisa de tratamento manual dos dados antes de prosseguir.
            migrationBuilder.Sql(
                "UPDATE uc SET uc.Permissao = c.Nome " +
                "FROM UsuariosComercios uc " +
                "JOIN Cargos c ON c.CargoID = uc.CargoID");

            migrationBuilder.AlterColumn<string>(
                name: "Permissao",
                table: "UsuariosComercios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UsuariosComercios_Permissao",
                table: "UsuariosComercios",
                sql: "[Permissao] IN ('Administrador', 'Dono', 'Funcionario')");

            migrationBuilder.DropColumn(
                name: "CargoID",
                table: "UsuariosComercios");

            migrationBuilder.DropTable(
                name: "Cargos");
        }
    }
}
