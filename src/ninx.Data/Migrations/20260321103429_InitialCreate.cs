using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasProduto",
                columns: table => new
                {
                    CategoriaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasProduto", x => x.CategoriaID);
                });

            migrationBuilder.CreateTable(
                name: "Comercios",
                columns: table => new
                {
                    ComercioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endereco = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CNPJ = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercios", x => x.ComercioID);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Login = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SenhaHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Permissao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioID);
                });

            migrationBuilder.CreateTable(
                name: "Assinaturas",
                columns: table => new
                {
                    AssinaturaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    Plano = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "date", nullable: false),
                    DataFim = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Ativa"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinaturas", x => x.AssinaturaID);
                    table.ForeignKey(
                        name: "FK_Assinaturas_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    ClienteID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LimiteCredito = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.ClienteID);
                    table.ForeignKey(
                        name: "FK_Clientes_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    ProdutoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    CategoriaID = table.Column<int>(type: "int", nullable: true),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CodigoBarras = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrecoVenda = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PrecoCusto = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    UnidadeMedida = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "UN"),
                    Validade = table.Column<DateTime>(type: "date", nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.ProdutoID);
                    table.ForeignKey(
                        name: "FK_Produtos_CategoriasProduto_CategoriaID",
                        column: x => x.CategoriaID,
                        principalTable: "CategoriasProduto",
                        principalColumn: "CategoriaID");
                    table.ForeignKey(
                        name: "FK_Produtos_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SessoesWhatsapp",
                columns: table => new
                {
                    SessaoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    NumeroCelular = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Etapa = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DadosTemporarios = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UltimaInteracao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesWhatsapp", x => x.SessaoID);
                    table.ForeignKey(
                        name: "FK_SessoesWhatsapp_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioComercios",
                columns: table => new
                {
                    UsuarioComercioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    Permissao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioComercios", x => x.UsuarioComercioID);
                    table.ForeignKey(
                        name: "FK_UsuarioComercios_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioComercios_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    VendaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TipoVenda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Aberta"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.VendaID);
                    table.ForeignKey(
                        name: "FK_Vendas_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendas_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Estoque",
                columns: table => new
                {
                    EstoqueID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(10,3)", nullable: false, defaultValue: 0m),
                    QuantidadeMinima = table.Column<decimal>(type: "decimal(10,3)", nullable: false, defaultValue: 0m),
                    UltimaAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estoque", x => x.EstoqueID);
                    table.ForeignKey(
                        name: "FK_Estoque_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Estoque_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    MovimentacaoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    ReferenciaID = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.MovimentacaoID);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItensVenda",
                columns: table => new
                {
                    ItemVendaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendaID = table.Column<int>(type: "int", nullable: false),
                    ProdutoID = table.Column<int>(type: "int", nullable: false),
                    ProdutoNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProdutoCodigoBarras = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnidadeMedida = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantidade = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensVenda", x => x.ItemVendaID);
                    table.ForeignKey(
                        name: "FK_ItensVenda_Produtos_ProdutoID",
                        column: x => x.ProdutoID,
                        principalTable: "Produtos",
                        principalColumn: "ProdutoID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItensVenda_Vendas_VendaID",
                        column: x => x.VendaID,
                        principalTable: "Vendas",
                        principalColumn: "VendaID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosVenda",
                columns: table => new
                {
                    PagamentoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendaID = table.Column<int>(type: "int", nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosVenda", x => x.PagamentoID);
                    table.ForeignKey(
                        name: "FK_PagamentosVenda_Vendas_VendaID",
                        column: x => x.VendaID,
                        principalTable: "Vendas",
                        principalColumn: "VendaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendasFiado",
                columns: table => new
                {
                    VendaFiadoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendaID = table.Column<int>(type: "int", nullable: false),
                    ClienteID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Pendente"),
                    Assinatura = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DocumentoPDF = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendasFiado", x => x.VendaFiadoID);
                    table.ForeignKey(
                        name: "FK_VendasFiado_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "ClienteID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VendasFiado_Vendas_VendaID",
                        column: x => x.VendaID,
                        principalTable: "Vendas",
                        principalColumn: "VendaID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosFiado",
                columns: table => new
                {
                    PagamentoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendaFiadoID = table.Column<int>(type: "int", nullable: false),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Observacao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosFiado", x => x.PagamentoID);
                    table.ForeignKey(
                        name: "FK_PagamentosFiado_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagamentosFiado_VendasFiado_VendaFiadoID",
                        column: x => x.VendaFiadoID,
                        principalTable: "VendasFiado",
                        principalColumn: "VendaFiadoID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_ComercioID",
                table: "Assinaturas",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_ComercioID",
                table: "Clientes",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_Comercios_CNPJ",
                table: "Comercios",
                column: "CNPJ",
                unique: true,
                filter: "[CNPJ] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_ComercioID",
                table: "Estoque",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_ProdutoID",
                table: "Estoque",
                column: "ProdutoID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_ProdutoID_ComercioID",
                table: "Estoque",
                columns: new[] { "ProdutoID", "ComercioID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_ProdutoID",
                table: "ItensVenda",
                column: "ProdutoID");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_VendaID",
                table: "ItensVenda",
                column: "VendaID");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ComercioID",
                table: "MovimentacoesEstoque",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_ProdutoID",
                table: "MovimentacoesEstoque",
                column: "ProdutoID");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentacoesEstoque_UsuarioID",
                table: "MovimentacoesEstoque",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosFiado_UsuarioID",
                table: "PagamentosFiado",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosFiado_VendaFiadoID",
                table: "PagamentosFiado",
                column: "VendaFiadoID");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosVenda_VendaID",
                table: "PagamentosVenda",
                column: "VendaID");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_CategoriaID",
                table: "Produtos",
                column: "CategoriaID");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_ComercioID_CodigoBarras",
                table: "Produtos",
                columns: new[] { "ComercioID", "CodigoBarras" },
                unique: true,
                filter: "[CodigoBarras] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesWhatsapp_ComercioID",
                table: "SessoesWhatsapp",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioComercios_ComercioID",
                table: "UsuarioComercios",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioComercios_UsuarioID",
                table: "UsuarioComercios",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Login",
                table: "Usuarios",
                column: "Login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ComercioID",
                table: "Vendas",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_UsuarioID",
                table: "Vendas",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_VendasFiado_ClienteID",
                table: "VendasFiado",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_VendasFiado_VendaID",
                table: "VendasFiado",
                column: "VendaID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assinaturas");

            migrationBuilder.DropTable(
                name: "Estoque");

            migrationBuilder.DropTable(
                name: "ItensVenda");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "PagamentosFiado");

            migrationBuilder.DropTable(
                name: "PagamentosVenda");

            migrationBuilder.DropTable(
                name: "SessoesWhatsapp");

            migrationBuilder.DropTable(
                name: "UsuarioComercios");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "VendasFiado");

            migrationBuilder.DropTable(
                name: "CategoriasProduto");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "Comercios");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
