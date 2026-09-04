using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ninx.Data.Context;
using ninx.Domain.Entities;
using Xunit;

namespace ninx.Tests.Integration
{
    /// <summary>
    /// Teste de regressão de isolamento entre comércios (tenants): cria dois comércios distintos,
    /// cada um com seu próprio cliente, autentica como usuário do comércio A e garante que ele
    /// não consegue ler/acessar dados do comércio B através da API — mesmo sabendo o ID do recurso.
    /// O histórico do projeto já registrou uma falha real de isolamento entre tenants; este teste
    /// existe para travar essa regressão.
    /// </summary>
    public class TenantIsolationTests : IClassFixture<NinxWebApplicationFactory>
    {
        private readonly NinxWebApplicationFactory _factory;

        public TenantIsolationTests(NinxWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task UsuarioDoComercioA_NaoDeveAcessarClienteDoComercioB_PorGetById()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();

            var (usuarioA, cargoA, comercioA, _) = IntegrationTestHelpers.SeedComercioComUsuario(
                db, emailUsuario: "tenantA@teste.com", nomeComercio: "Comércio A");
            var (_, _, comercioB, _) = IntegrationTestHelpers.SeedComercioComUsuario(
                db, emailUsuario: "tenantB@teste.com", nomeComercio: "Comércio B");

            var clienteB = new Cliente
            {
                ComercioID = comercioB.ComercioID,
                Nome = "Cliente do Comércio B",
                Cpf = "52998224725",
                EnderecoLogradouro = "Rua B",
                EnderecoNumero = "1",
                EnderecoBairro = "Bairro B",
                EnderecoCidade = "Cidade B",
                EnderecoUF = "SP",
                EnderecoCEP = "01000000",
                Ativo = true
            };
            db.Clientes.Add(clienteB);
            db.SaveChanges();

            var tokenA = IntegrationTestHelpers.GerarTokenPara(_factory, usuarioA, cargoA, comercioA.ComercioID, comercioA.NomeComercio);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

            var response = await client.GetAsync($"/api/Cliente/{clienteB.ClienteID}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound,
                "um usuário autenticado no comércio A não deve conseguir ler um cliente que pertence ao comércio B");
        }

        [Fact]
        public async Task UsuarioDoComercioA_ListaDeClientes_NaoDeveIncluirClientesDoComercioB()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();

            var (usuarioA, cargoA, comercioA, _) = IntegrationTestHelpers.SeedComercioComUsuario(
                db, emailUsuario: "tenantA-lista@teste.com", nomeComercio: "Comércio A Lista");
            var (_, _, comercioB, _) = IntegrationTestHelpers.SeedComercioComUsuario(
                db, emailUsuario: "tenantB-lista@teste.com", nomeComercio: "Comércio B Lista");

            db.Clientes.Add(new Cliente
            {
                ComercioID = comercioA.ComercioID,
                Nome = "Cliente do Comércio A",
                Cpf = "52998224725",
                EnderecoLogradouro = "Rua A",
                EnderecoNumero = "1",
                EnderecoBairro = "Bairro A",
                EnderecoCidade = "Cidade A",
                EnderecoUF = "SP",
                EnderecoCEP = "01000000",
                Ativo = true
            });
            db.Clientes.Add(new Cliente
            {
                ComercioID = comercioB.ComercioID,
                Nome = "Cliente Exclusivo do Comércio B",
                Cpf = "11144477735",
                EnderecoLogradouro = "Rua B",
                EnderecoNumero = "2",
                EnderecoBairro = "Bairro B",
                EnderecoCidade = "Cidade B",
                EnderecoUF = "RJ",
                EnderecoCEP = "02000000",
                Ativo = true
            });
            db.SaveChanges();

            var tokenA = IntegrationTestHelpers.GerarTokenPara(_factory, usuarioA, cargoA, comercioA.ComercioID, comercioA.NomeComercio);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

            var response = await client.GetAsync("/api/Cliente/All?PageSize=100");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // PaginatedResponse<T> não tem construtor parameterless (só construtores com parâmetros),
            // o que o desserializador padrão do System.Text.Json não suporta. Em vez de depender do
            // shape exato do envelope de paginação, inspeciona-se o JSON cru pelo array "data".
            var json = await response.Content.ReadAsStringAsync();
            using var documento = JsonDocument.Parse(json);
            var nomes = documento.RootElement.GetProperty("data")
                .EnumerateArray()
                .Select(c => c.GetProperty("nome").GetString())
                .ToList();

            nomes.Should().NotContain("Cliente Exclusivo do Comércio B");
            nomes.Should().Contain("Cliente do Comércio A");
        }
    }
}
