using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaLimiteCreditoPadraoComercio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LimiteCreditoPadrao",
                table: "Comercios",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LimiteCreditoPadrao",
                table: "Comercios");
        }
    }
}
