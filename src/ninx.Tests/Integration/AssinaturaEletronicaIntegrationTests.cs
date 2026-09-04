using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using ninx.Communication;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using Xunit;

namespace ninx.Tests.Integration
{
    /// <summary>
    /// Testes dos endpoints públicos ([AllowAnonymous]) de assinatura eletrônica, usados pelo
    /// assinante do documento — não devem exigir token JWT.
    /// </summary>
    public class AssinaturaEletronicaIntegrationTests : IClassFixture<NinxWebApplicationFactory>
    {
        private readonly NinxWebApplicationFactory _factory;

        public AssinaturaEletronicaIntegrationTests(NinxWebApplicationFactory factory)
        {
            _factory = factory;
        }

        private (NinxDB Db, Venda Venda, AssinaturaEletronica Assinatura) Seed(string sufixo)
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
            db.Database.EnsureCreated();
            var (usuario, _, comercio, _) = IntegrationTestHelpers.SeedComercioComUsuario(db, emailUsuario: $"assinatura-{sufixo}@teste.com");

            var venda = new Venda
            {
                ComercioID = comercio.ComercioID,
                UsuarioID = usuario.UsuarioID,
                Total = 100m,
                Status = StatusVenda.Aguardando,
                TipoVenda = TipoVenda.Fiado,
                CriadoEm = DateTime.UtcNow
            };
            db.Vendas.Add(venda);
            db.SaveChanges();

            var assinatura = new AssinaturaEletronica
            {
                VendaID = venda.VendaID,
                DocumentoGuid = Guid.NewGuid(),
                TipoDocumento = TipoDocumento.TermoCompromisso,
                DocumentoHtmlMesclado = "<html><!--BLOCO_ASSINATURA_INICIO--><!--BLOCO_ASSINATURA_FIM--></html>",
                DocumentoOriginalBase64 = "base64",
                Assinado = false,
                Status = StatusAssinatura.Ativa
            };
            db.AssinaturaEletronica.Add(assinatura);
            db.SaveChanges();

            return (db, venda, assinatura);
        }

        [Fact]
        public async Task ObterDocumento_SemToken_DeveRetornarDadosDoDocumento()
        {
            var (_, _, assinatura) = Seed("obter");
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/AssinaturaEletronica/{assinatura.DocumentoGuid}");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var body = await response.Content.ReadFromJsonAsync<AssinaturaEletronicaResponse>();
            body!.DocumentoGuid.Should().Be(assinatura.DocumentoGuid);
        }

        [Fact]
        public async Task ObterDocumento_GuidInexistente_DeveRetornar404()
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/AssinaturaEletronica/{Guid.NewGuid()}");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task ValidaAssinado_DocumentoAindaNaoAssinado_DeveRetornarBadRequest()
        {
            var (_, _, assinatura) = Seed("naoassinado");
            var client = _factory.CreateClient();

            var response = await client.GetAsync($"/api/AssinaturaEletronica/assinado/{assinatura.DocumentoGuid}");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task ConfirmarAssinatura_SemToken_DeveAssinarERefletirEmValidaAssinado()
        {
            var (_, venda, assinatura) = Seed("confirmar");
            var client = _factory.CreateClient();

            var confirmar = await client.PostAsJsonAsync(
                $"/api/AssinaturaEletronica/confirmar/{assinatura.DocumentoGuid}",
                new ConfirmarAssinaturaEletronicaRequest { ImagemBase64 = "QXNzaW5hdHVyYURlVGVzdGU=" });

            confirmar.StatusCode.Should().Be(HttpStatusCode.OK);

            var validar = await client.GetAsync($"/api/AssinaturaEletronica/assinado/{assinatura.DocumentoGuid}");
            validar.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task ConfirmarAssinatura_GuidInexistente_DeveRetornar404()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync(
                $"/api/AssinaturaEletronica/confirmar/{Guid.NewGuid()}",
                new ConfirmarAssinaturaEletronicaRequest { ImagemBase64 = "QXNzaW5hdHVyYURlVGVzdGU=" });

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
