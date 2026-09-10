using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEnderecoDetalhadoComercio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnderecoBairro",
                table: "Comercios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCEP",
                table: "Comercios",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCidade",
                table: "Comercios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoComplemento",
                table: "Comercios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoLogradouro",
                table: "Comercios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoNumero",
                table: "Comercios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoUF",
                table: "Comercios",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnderecoBairro",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "EnderecoCEP",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "EnderecoCidade",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "EnderecoComplemento",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "EnderecoLogradouro",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "EnderecoNumero",
                table: "Comercios");

            migrationBuilder.DropColumn(
                name: "EnderecoUF",
                table: "Comercios");
        }
    }
}
