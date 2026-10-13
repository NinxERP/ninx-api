using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ninx.Data.Context;
using Xunit;

namespace ninx.Tests.Integration
{
    /// <summary>
    /// A interface esconde os relatórios de quem não tem a permissão VisualizarRelatorios; estes
    /// testes garantem que a API também recusa, para a regra não depender só da interface.
    /// </summary>
    public class RelatorioPermissaoTests : IClassFixture<NinxWebApplicationFactory>
    {
        private readonly NinxWebApplicationFactory _factory;

        public RelatorioPermissaoTests(NinxWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task CargoSemPermissao_NaoDeveAcessarRelatorios()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();
            var (usuario, cargo, comercio, _) = IntegrationTestHelpers.SeedComercioComUsuario(
                db, emailUsuario: "caixa-relatorio@teste.com", nomeComercio: "Comércio Relatório Caixa", ehProprietario: false);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                IntegrationTestHelpers.GerarTokenPara(_factory, usuario, cargo, comercio.ComercioID, comercio.NomeComercio));

            var response = await client.GetAsync("/api/Relatorio/limite-credito");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Proprietario_DeveAcessarRelatorios()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();
            var (usuario, cargo, comercio, _) = IntegrationTestHelpers.SeedComercioComUsuario(
                db, emailUsuario: "dono-relatorio@teste.com", nomeComercio: "Comércio Relatório Dono");

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
                IntegrationTestHelpers.GerarTokenPara(_factory, usuario, cargo, comercio.ComercioID, comercio.NomeComercio));

            var response = await client.GetAsync("/api/Relatorio/limite-credito");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
