using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ninx.Communication;
using ninx.Communication.Venda;
using ninx.Data.Context;
using ninx.Domain.Entities;
using Xunit;

namespace ninx.Tests.Integration
{
    /// <summary>
    /// Criação de venda ponta a ponta: HTTP -> Controller -> VendaService -> repositórios reais
    /// (EF Core InMemory) -> persistência.
    /// </summary>
    public class VendaIntegrationTests : IClassFixture<NinxWebApplicationFactory>
    {
        private readonly NinxWebApplicationFactory _factory;

        public VendaIntegrationTests(NinxWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CriarVenda_Normal_DeveCriarComSucessoEDebitarEstoque()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();
            var (usuario, cargo, comercio, _) = IntegrationTestHelpers.SeedComercioComUsuario(db, emailUsuario: "venda-e2e@teste.com");

            var produto = new Produto { ComercioID = comercio.ComercioID, Nome = "Produto Venda", PrecoVenda = 15m, Ativo = true };
            db.Produtos.Add(produto);
            db.SaveChanges();

            db.Estoques.Add(new Estoque { ComercioID = comercio.ComercioID, ProdutoID = produto.ProdutoID, Quantidade = 50, RowVersion = new byte[] { 1 } });
            db.SaveChanges();

            var token = IntegrationTestHelpers.GerarTokenPara(_factory, usuario, cargo, comercio.ComercioID, comercio.NomeComercio);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var request = new CriarVendaRequest
            {
                ComercioID = comercio.ComercioID,
                UsuarioID = usuario.UsuarioID,
                ClienteID = 0,
                TipoVenda = 1,
                ItensVenda = new List<ItemVendaRequest> { new() { ProdutoID = produto.ProdutoID, Quantidade = 3 } },
                Pagamentos = new List<PagamentoVendaRequest> { new() { FormaPagamento = 1, Valor = 45m } }
            };

            var response = await client.PostAsJsonAsync("/api/Venda", request);

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var body = await response.Content.ReadFromJsonAsync<VendaResponse>();
            body!.Total.Should().Be(45m);

            using var scopeVerificacao = _factory.Services.CreateScope();
            var dbVerificacao = scopeVerificacao.ServiceProvider.GetRequiredService<NinxDB>();
            var estoqueAtualizado = dbVerificacao.Estoques.First(e => e.ProdutoID == produto.ProdutoID);
            estoqueAtualizado.Quantidade.Should().Be(47);
        }

        [Fact]
        public async Task CriarVenda_SemAutenticacao_DeveRetornar401()
        {
            var client = _factory.CreateClient();
            var request = new CriarVendaRequest
            {
                ComercioID = 1,
                UsuarioID = 1,
                TipoVenda = 1,
                ItensVenda = new List<ItemVendaRequest> { new() { ProdutoID = 1, Quantidade = 1 } },
                Pagamentos = new List<PagamentoVendaRequest> { new() { FormaPagamento = 1, Valor = 1 } }
            };

            var response = await client.PostAsJsonAsync("/api/Venda", request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
