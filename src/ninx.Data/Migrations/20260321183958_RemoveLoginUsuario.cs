using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLoginUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PagamentosVenda_Vendas_VendaID",
                table: "PagamentosVenda");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Login",
                table: "Usuarios");

            migrationBuilder.AddForeignKey(
                name: "FK_PagamentosVenda_Vendas_VendaID",
                table: "PagamentosVenda",
                column: "VendaID",
                principalTable: "Vendas",
                principalColumn: "VendaID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PagamentosVenda_Vendas_VendaID",
                table: "PagamentosVenda");

            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "Usuarios",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios",
                column: "Login",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PagamentosVenda_Vendas_VendaID",
                table: "PagamentosVenda",
                column: "VendaID",
                principalTable: "Vendas",
                principalColumn: "VendaID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
