using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AtualizarConstraintEStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vendas_Status",
                table: "Vendas");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vendas_Status",
                table: "Vendas",
                sql: "[Status] IN ('Aberta', 'Finalizada', 'Cancelada', 'Estornada', 'Aguardando')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Vendas_Status",
                table: "Vendas");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Vendas_Status",
                table: "Vendas",
                sql: "[Status] IN ('Aberta', 'Finalizada', 'Cancelada', 'Estornada')");
        }
    }
}
