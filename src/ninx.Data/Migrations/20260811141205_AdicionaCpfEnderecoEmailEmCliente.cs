using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCpfEnderecoEmailEmCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_ComercioID",
                table: "Clientes");

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "Clientes",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Clientes",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoBairro",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCEP",
                table: "Clientes",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCidade",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoComplemento",
                table: "Clientes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoLogradouro",
                table: "Clientes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoNumero",
                table: "Clientes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoUF",
                table: "Clientes",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ComercioID_Cpf",
                table: "Clientes",
                columns: new[] { "ComercioID", "Cpf" },
                unique: true,
                filter: "[Cpf] <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clientes_ComercioID_Cpf",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoBairro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoCEP",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoCidade",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoComplemento",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoLogradouro",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoNumero",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoUF",
                table: "Clientes");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ComercioID",
                table: "Clientes",
                column: "ComercioID");
        }
    }
}
