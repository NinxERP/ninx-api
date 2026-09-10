using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCargoAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Reservado",
                table: "Cargos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Cargo reservado usado apenas na sessão de administradores de plataforma (Usuario.Admin == true):
            // peso máximo garante prioridade sobre qualquer cargo customizado; Reservado impede edição/desativação
            // e exclusão das listagens de atribuição manual (ver CargoService/CargoRepository).
            migrationBuilder.Sql(
                "INSERT INTO Cargos (Nome, Peso, ComercioID, Ativo, CriadoEm, Reservado) VALUES " +
                "('Admin', 2147483647, NULL, 1, GETUTCDATE(), 1)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversão só é segura se nenhum UsuarioComercio estiver usando esse CargoID (não deveria, já que o
            // cargo é excluído das listagens de atribuição) — se estiver, a FK vai bloquear o DELETE abaixo.
            migrationBuilder.Sql(
                "DELETE FROM Cargos WHERE Nome = 'Admin' AND ComercioID IS NULL AND Reservado = 1");

            migrationBuilder.DropColumn(
                name: "Reservado",
                table: "Cargos");
        }
    }
}
