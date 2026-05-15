using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ninx.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
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
                    NomeComercio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Endereco = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CNPJ = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: true),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
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
                    SenhaHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Permissao = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioID);
                    table.CheckConstraint("CK_Usuarios_Permissao", "[Permissao] IN ('Administrador', 'Dono', 'Funcionario')");
                });

            migrationBuilder.CreateTable(
                name: "AssinaturaPlano",
                columns: table => new
                {
                    AssinaturaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    Plano = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "date", nullable: false),
                    DataFim = table.Column<DateTime>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Ativa"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assinaturas", x => x.AssinaturaID);
                    table.CheckConstraint("CK_Assinaturas_Plano", "[Plano] IN ('Mensal', 'Trimestral', 'Anual')");
                    table.CheckConstraint("CK_Assinaturas_Status", "[Status] IN ('Ativa', 'Vencida', 'Cancelada')");
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
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
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
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.ProdutoID);
                    table.CheckConstraint("CK_Produtos_UnidadeMedida", "[UnidadeMedida] IN ('UN', 'KG', 'L', 'G')");
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
                    Etapa = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Menu"),
                    DadosTemporarios = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UltimaInteracao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesWhatsapp", x => x.SessaoID);
                    table.CheckConstraint("CK_SessoesWhatsapp_Etapa", "[Etapa] IN ('Menu', 'AguardandoProduto', 'AguardandoSelecao')");
                    table.ForeignKey(
                        name: "FK_SessoesWhatsapp_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuariosComercios",
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
                    table.PrimaryKey("PK_UsuariosComercios", x => x.UsuarioComercioID);
                    table.CheckConstraint("CK_UsuariosComercios_Permissao", "[Permissao] IN ('Administrador', 'Dono', 'Funcionario')");
                    table.ForeignKey(
                        name: "FK_UsuariosComercios_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuariosComercios_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PagamentosAssinaturasPlano",
                columns: table => new
                {
                    PagamentoAssinaturaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssinaturaID = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    DataPagamento = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DataVencimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosAssinaturasPlano", x => x.PagamentoAssinaturaID);
                    table.ForeignKey(
                        name: "FK_PagamentosAssinaturasPlano_Assinaturas_AssinaturaID",
                        column: x => x.AssinaturaID,
                        principalTable: "Assinaturas",
                        principalColumn: "AssinaturaID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    VendaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComercioID = table.Column<int>(type: "int", nullable: false),
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    ClienteID = table.Column<int>(type: "int", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Aberta"),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TipoVenda = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.VendaID);
                    table.CheckConstraint("CK_Vendas_Status", "[Status] IN ('Aberta', 'Finalizada', 'Cancelada', 'Estornada')");
                    table.CheckConstraint("CK_Vendas_TipoVenda", "[TipoVenda] IN ('Normal', 'Fiado')");
                    table.ForeignKey(
                        name: "FK_Vendas_Clientes_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Clientes",
                        principalColumn: "ClienteID",
                        onDelete: ReferentialAction.Restrict);
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
                    UltimaAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
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
                name: "AssinaturasEletronicas",
                columns: table => new
                {
                    AssinaturaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendaID = table.Column<int>(type: "int", nullable: false),
                    DocumentoGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImagemAssinatura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DataAssinatura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IpAssinante = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    DispositivoInfo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Assinado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Ativa")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssinaturasEletronicas", x => x.AssinaturaID);
                    table.CheckConstraint("CK_AssinaturasEletronicas_Status", "[Status] IN ('Ativa', 'Vencida', 'Cancelada')");
                    table.ForeignKey(
                        name: "FK_AssinaturasEletronicas_Vendas_VendaID",
                        column: x => x.VendaID,
                        principalTable: "Vendas",
                        principalColumn: "VendaID",
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
                    table.CheckConstraint("CK_ItensVenda_UnidadeMedida", "[UnidadeMedida] IN ('UN', 'KG', 'L', 'G')");
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
                    VendaID = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DataHora = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.MovimentacaoID);
                    table.CheckConstraint("CK_MovimentacoesEstoque_Tipo", "[Tipo] IN ('Entrada', 'Venda', 'Ajuste', 'Perda', 'Devolucao', 'Estorno')");
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Comercios_ComercioID",
                        column: x => x.ComercioID,
                        principalTable: "Comercios",
                        principalColumn: "ComercioID",
                        onDelete: ReferentialAction.Restrict);
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
                    table.ForeignKey(
                        name: "FK_MovimentacoesEstoque_Vendas_VendaID",
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
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    PagamentoVinculoID = table.Column<int>(type: "int", nullable: false),
                    FormaPagamento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    AtualizadoEm = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    Status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "Pago")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosVenda", x => x.PagamentoID);
                    table.CheckConstraint("CK_PagamentosVenda_FormaPagamento", "[FormaPagamento] IN ('Dinheiro', 'Pix', 'Cartao')");
                    table.CheckConstraint("CK_PagamentosVenda_Status", "[Status] IN ('Pago', 'Estornado')");
                    table.ForeignKey(
                        name: "FK_PagamentosVenda_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PagamentosVenda_Vendas_VendaID",
                        column: x => x.VendaID,
                        principalTable: "Vendas",
                        principalColumn: "VendaID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assinaturas_ComercioID",
                table: "Assinaturas",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_AssinaturasEletronicas_VendaID",
                table: "AssinaturasEletronicas",
                column: "VendaID");

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
                name: "IX_MovimentacoesEstoque_VendaID",
                table: "MovimentacoesEstoque",
                column: "VendaID");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosAssinaturasPlano_AssinaturaID",
                table: "PagamentosAssinaturasPlano",
                column: "AssinaturaID");

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosVenda_UsuarioID",
                table: "PagamentosVenda",
                column: "UsuarioID");

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
                name: "IX_SessoesWhatsapp_ComercioID_NumeroCelular",
                table: "SessoesWhatsapp",
                columns: new[] { "ComercioID", "NumeroCelular" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosComercios_ComercioID",
                table: "UsuariosComercios",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_UsuariosComercios_UsuarioID_ComercioID",
                table: "UsuariosComercios",
                columns: new[] { "UsuarioID", "ComercioID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ClienteID",
                table: "Vendas",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ComercioID",
                table: "Vendas",
                column: "ComercioID");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_UsuarioID",
                table: "Vendas",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssinaturasEletronicas");

            migrationBuilder.DropTable(
                name: "Estoque");

            migrationBuilder.DropTable(
                name: "ItensVenda");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "PagamentosAssinaturasPlano");

            migrationBuilder.DropTable(
                name: "PagamentosVenda");

            migrationBuilder.DropTable(
                name: "SessoesWhatsapp");

            migrationBuilder.DropTable(
                name: "UsuariosComercios");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Assinaturas");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "CategoriasProduto");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Comercios");
        }
    }
}
