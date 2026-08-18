using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaLogAuditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    LogAuditoriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    ComercioID = table.Column<int>(type: "int", nullable: true),
                    Acao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Entidade = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntidadeID = table.Column<int>(type: "int", nullable: false),
                    Detalhes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.LogAuditoriaID);
                    table.ForeignKey(
                        name: "FK_LogsAuditoria_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID");
                    table.ForeignKey(
                        name: "FK_LogsAuditoria_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_ComercioID",
                table: "LogsAuditoria",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_CriadoEm",
                table: "LogsAuditoria",
                column: "CriadoEm");

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_UsuarioID",
                table: "LogsAuditoria",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LogsAuditoria");
        }
    }
}
