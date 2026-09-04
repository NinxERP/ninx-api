using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ninx.Communication;
using ninx.Data.Context;
using ninx.Domain.Entities;
using Xunit;

namespace ninx.Tests.Integration
{
    /// <summary>
    /// Teste de regressão: a rota de busca de cliente por id foi alterada de
    /// "GET api/Cliente/id/{id}" para "GET api/Cliente/{id}" (padronizando com
    /// Produto/Estoque/Comercio). Garante que a nova rota resolve para a action GetById
    /// e não cai em 404 por descompasso de rota.
    /// </summary>
    public class ClienteRouteRegressionTests : IClassFixture<NinxWebApplicationFactory>
    {
        private readonly NinxWebApplicationFactory _factory;

        public ClienteRouteRegressionTests(NinxWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetById_RotaSemSegmentoId_DeveResolverParaGetByIdEDevolverOCliente()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();
            var (usuario, cargo, comercio, _) = IntegrationTestHelpers.SeedComercioComUsuario(db, emailUsuario: "cliente-rota@teste.com");

            var cliente = new Cliente
            {
                ComercioID = comercio.ComercioID,
                Nome = "Cliente Rota",
                Cpf = "52998224725",
                EnderecoLogradouro = "Rua X",
                EnderecoNumero = "1",
                EnderecoBairro = "Bairro",
                EnderecoCidade = "Cidade",
                EnderecoUF = "SP",
                EnderecoCEP = "01000000",
                Ativo = true
            };
            db.Clientes.Add(cliente);
            db.SaveChanges();

            var token = IntegrationTestHelpers.GerarTokenPara(_factory, usuario, cargo, comercio.ComercioID, comercio.NomeComercio);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/api/Cliente/{cliente.ClienteID}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<ClienteResponse>();
            body!.ClienteID.Should().Be(cliente.ClienteID);
            body.Nome.Should().Be("Cliente Rota");
        }

        [Fact]
        public async Task GetById_ClienteInexistente_DeveRetornar404ENaoDescasarRota()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();
            var (usuario, cargo, comercio, _) = IntegrationTestHelpers.SeedComercioComUsuario(db, emailUsuario: "cliente-rota-404@teste.com");

            var token = IntegrationTestHelpers.GerarTokenPara(_factory, usuario, cargo, comercio.ComercioID, comercio.NomeComercio);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/api/Cliente/999999");

            // 404 vindo da lógica de negócio (NotFoundException -> ExceptionMiddleware), não do roteamento.
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
            var body = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            body!.Messagem.Should().NotBeNullOrWhiteSpace();
        }
    }
}
