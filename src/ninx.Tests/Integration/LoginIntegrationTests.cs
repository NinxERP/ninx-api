using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ninx.Communication;
using ninx.Data.Context;
using Xunit;

namespace ninx.Tests.Integration
{
    public class LoginIntegrationTests : IClassFixture<NinxWebApplicationFactory>
    {
        private readonly NinxWebApplicationFactory _factory;

        public LoginIntegrationTests(NinxWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_CredenciaisValidas_DeveRetornarTokenJwt()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();
            IntegrationTestHelpers.SeedComercioComUsuario(db, emailUsuario: "login-ok@teste.com");

            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/Login", new LoginRequest { Email = "login-ok@teste.com", Senha = "Senha123!" });

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
            body!.Token.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Login_SenhaIncorreta_DeveRetornarBadRequest()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();
            IntegrationTestHelpers.SeedComercioComUsuario(db, emailUsuario: "login-senha-errada@teste.com");

            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/Login", new LoginRequest { Email = "login-senha-errada@teste.com", Senha = "senhaErrada" });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Login_UsuarioInexistente_DeveRetornarBadRequest()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/Login", new LoginRequest { Email = "ninguem-existe@teste.com", Senha = "qualquer" });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task EndpointsAutenticados_SemToken_DeveRetornar401()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/Cliente/All");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
