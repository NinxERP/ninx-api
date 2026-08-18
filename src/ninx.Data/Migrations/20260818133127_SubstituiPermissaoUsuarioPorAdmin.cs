using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubstituiPermissaoUsuarioPorAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Admin",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Usuarios SET Admin = 1 WHERE Permissao = 'Administrador';");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Usuarios_Permissao",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Permissao",
                table: "Usuarios");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Permissao",
                table: "Usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Funcionario");

            migrationBuilder.Sql("UPDATE Usuarios SET Permissao = CASE WHEN Admin = 1 THEN 'Administrador' ELSE 'Funcionario' END;");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Usuarios_Permissao",
                table: "Usuarios",
                sql: "[Permissao] IN ('Administrador', 'Dono', 'Funcionario')");

            migrationBuilder.DropColumn(
                name: "Admin",
                table: "Usuarios");
        }
    }
}
