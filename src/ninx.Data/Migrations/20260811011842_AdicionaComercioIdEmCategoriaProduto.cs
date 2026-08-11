using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaComercioIdEmCategoriaProduto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComercioID",
                table: "CategoriasProduto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasProduto_ComercioID",
                table: "CategoriasProduto",
                column: "ComercioID");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriasProduto_Comercios_ComercioID",
                table: "CategoriasProduto",
                column: "ComercioID",
                principalTable: "Comercios",
                principalColumn: "ComercioID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriasProduto_Comercios_ComercioID",
                table: "CategoriasProduto");

            migrationBuilder.DropIndex(
                name: "IX_CategoriasProduto_ComercioID",
                table: "CategoriasProduto");

            migrationBuilder.DropColumn(
                name: "ComercioID",
                table: "CategoriasProduto");
        }
    }
}
