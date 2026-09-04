using FluentAssertions;
using Mapster;
using ninx.Application.Mappings;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using Xunit;

namespace ninx.Tests.Mappings
{
    /// <summary>
    /// Teste de regressão: AssinaturaEletronicaResponse.Filename é preenchido pelo BuildFilename
    /// (Mappings/AssinaturaEletronica/AssinaturaEletronicaMapper.cs), a partir do TipoDocumento + VendaID
    /// da entidade AssinaturaEletronica. Um TypeAdapterConfig isolado é usado (em vez do GlobalSettings)
    /// para não depender de scan de assembly nem interferir com outros testes.
    /// </summary>
    public class AssinaturaEletronicaMapperTests
    {
        private static AssinaturaEletronicaResponse Mapear(AssinaturaEletronica entidade)
        {
            var config = new TypeAdapterConfig();
            new AssinaturaEletronicaMapper().Register(config);
            return entidade.Adapt<AssinaturaEletronicaResponse>(config);
        }

        private static AssinaturaEletronica NovaAssinatura(TipoDocumento? tipoDocumento, int vendaId = 42) => new()
        {
            AssinaturaID = 1,
            VendaID = vendaId,
            DocumentoGuid = Guid.NewGuid(),
            TipoDocumento = tipoDocumento,
            DocumentoOriginalBase64 = "base64original",
            DocumentoAssinadoBase64 = "base64assinado",
            ImagemAssinatura = "base64assinatura",
            Assinado = false
        };

        [Theory]
        [InlineData(TipoDocumento.TermoCompromisso, "Termo de Compromisso")]
        [InlineData(TipoDocumento.ReciboPagamentoParcial, "Recibo de Pagamento Parcial")]
        [InlineData(TipoDocumento.ReciboQuitacaoGlobal, "Recibo de Quitação Global")]
        public void BuildFilename_ParaCadaTipoDeDocumento_DeveGerarNomeDescritivoComVendaId(TipoDocumento tipoDocumento, string prefixoEsperado)
        {
            var assinatura = NovaAssinatura(tipoDocumento, vendaId: 42);

            var response = Mapear(assinatura);

            response.Filename.Should().NotBeNullOrWhiteSpace();
            response.Filename.Should().Be($"{prefixoEsperado} - Venda 42.pdf");
        }

        [Fact]
        public void BuildFilename_TipoDocumentoNulo_DeveGerarNomeGenericoNaoVazio()
        {
            var assinatura = NovaAssinatura(tipoDocumento: null, vendaId: 7);

            var response = Mapear(assinatura);

            response.Filename.Should().NotBeNullOrWhiteSpace();
            response.Filename.Should().Be("Documento - Venda 7.pdf");
        }

        [Fact]
        public void Map_DeveTambemPreencherOsCamposDeDocumentoEAssinatura()
        {
            var assinatura = NovaAssinatura(TipoDocumento.TermoCompromisso);

            var response = Mapear(assinatura);

            response.DocumentoBase64.Should().Be("base64original");
            response.DocumentoAssinadoBase64.Should().Be("base64assinado");
            response.AssinaturaBase64.Should().Be("base64assinatura");
        }
    }
}
