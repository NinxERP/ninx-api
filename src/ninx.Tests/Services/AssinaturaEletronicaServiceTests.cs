using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class AssinaturaEletronicaServiceTests
    {
        private readonly Mock<IAssinaturaEletronicaRepository> _assinaturaRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IVendaRepository> _vendaRepository = new();
        private readonly Mock<IDocumentoRendererService> _documentoRendererService = new();
        private readonly Mock<IVendaService> _vendaService = new();
        private readonly Mock<IContaFiadoService> _contaFiadoService = new();

        // O portal envia só os traços; o renderer (mockado) devolve o original com a assinatura desenhada.
        private static readonly List<TracoAssinaturaRequest> Tracos = new()
        {
            new() { Pagina = 1, Espessura = 1.5, Pontos = new() { new() { X = 0.1, Y = 0.8 }, new() { X = 0.3, Y = 0.85 } } }
        };
        private static readonly string DocumentoAssinadoBase64 = Convert.ToBase64String("%PDF-1.7 documento assinado"u8.ToArray());

        public AssinaturaEletronicaServiceTests()
        {
            _documentoRendererService.Setup(x => x.AplicarTracosAssinatura(It.IsAny<string>(), It.IsAny<IReadOnlyCollection<TracoAssinaturaRequest>>()))
                .Returns(DocumentoAssinadoBase64);
        }

        private AssinaturaEletronicaService CriarService() => new(
            _assinaturaRepository.Object,
            _unitOfWork.Object,
            _vendaRepository.Object,
            _documentoRendererService.Object,
            _vendaService.Object,
            _contaFiadoService.Object);

        [Fact]
        public async Task ConfirmarAssinaturaAsync_DocumentoInexistente_DeveLancarNotFound()
        {
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>())).ReturnsAsync(new List<AssinaturaEletronica>());
            var service = CriarService();

            var act = async () => await service.ConfirmarAssinaturaAsync(Guid.NewGuid(), Tracos, "1.1.1.1", "device");

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_JaAssinado_DeveLancarBadRequest()
        {
            var assinatura = Builders.NovaAssinatura(assinado: true);
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });

            var service = CriarService();

            var act = async () => await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, Tracos, "1.1.1.1", "device");

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_StatusNaoAtivo_DeveLancarBadRequest()
        {
            var assinatura = Builders.NovaAssinatura(status: StatusAssinatura.Cancelada);
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });

            var service = CriarService();

            var act = async () => await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, Tracos, "1.1.1.1", "device");

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Theory]
        [InlineData(StatusVenda.Cancelada)]
        [InlineData(StatusVenda.Estornada)]
        public async Task ConfirmarAssinaturaAsync_VendaCanceladaOuEstornada_DeveLancarBadRequest(StatusVenda statusVenda)
        {
            var venda = Builders.NovaVenda(status: statusVenda);
            var assinatura = Builders.NovaAssinatura(vendaId: venda.VendaID);
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });
            _vendaService.Setup(x => x.EfetivarDocumentoAssinadoAsync(assinatura, It.IsAny<DateTime>()))
                .ThrowsAsync(new BadRequestException("venda cancelada"));
            _documentoRendererService.Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>())).ReturnsAsync("pdf");

            var service = CriarService();

            var act = async () => await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, Tracos, "1.1.1.1", "device");

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_Valida_DeveMarcarComoAssinadaEEfetivarVenda()
        {
            var venda = Builders.NovaVenda(status: StatusVenda.Aguardando);
            var assinatura = Builders.NovaAssinatura(vendaId: venda.VendaID);
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });
            _documentoRendererService.Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>())).ReturnsAsync("pdf");
            _documentoRendererService.Setup(x => x.AnexarPdfBase64(DocumentoAssinadoBase64, "pdf")).Returns("assinado+certificado");

            var service = CriarService();
            await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, Tracos, "1.1.1.1", "device");

            _documentoRendererService.Verify(x => x.AplicarTracosAssinatura(assinatura.DocumentoOriginalBase64, Tracos), Times.Once);
            assinatura.Assinado.Should().BeTrue();
            assinatura.ImagemAssinatura.Should().Be(DocumentoAssinadoBase64);
            assinatura.HashDocumentoAssinado.Should().Be(HashDocumento.Sha256Hex(DocumentoAssinadoBase64)).And.HaveLength(64);
            _vendaService.Verify(x => x.EfetivarDocumentoAssinadoAsync(assinatura, It.IsAny<DateTime>()), Times.Once);
            assinatura.DocumentoAssinadoBase64.Should().Be("assinado+certificado");
            _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_MultiplasVendasVinculadasAoMesmoGuid_DeveAssinarTodas()
        {
            var venda1 = Builders.NovaVenda(id: 1, status: StatusVenda.Aguardando);
            var venda2 = Builders.NovaVenda(id: 2, status: StatusVenda.Aguardando);
            var guid = Guid.NewGuid();
            var assinatura1 = Builders.NovaAssinatura(vendaId: 1, guid: guid);
            var assinatura2 = Builders.NovaAssinatura(vendaId: 2, guid: guid);

            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(guid))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura1, assinatura2 });
            _documentoRendererService.Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>())).ReturnsAsync("pdf");
            _documentoRendererService.Setup(x => x.AnexarPdfBase64(DocumentoAssinadoBase64, "pdf")).Returns("assinado+certificado");

            var service = CriarService();
            await service.ConfirmarAssinaturaAsync(guid, Tracos, "1.1.1.1", "device");

            assinatura1.Assinado.Should().BeTrue();
            assinatura2.Assinado.Should().BeTrue();
            assinatura1.HashDocumentoAssinado.Should().NotBeNull().And.Be(assinatura2.HashDocumentoAssinado);
            _assinaturaRepository.Verify(x => x.UpdateAsync(It.IsAny<AssinaturaEletronica>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_TermoDeAbertura_DeveEfetivarPelaContaDeFiado()
        {
            var assinatura = Builders.NovaAssinatura(tipoDocumento: TipoDocumento.TermoAberturaConta);
            assinatura.VendaID = null;
            assinatura.TermoAberturaID = 5;
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });
            _documentoRendererService.Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>())).ReturnsAsync("pdf");
            _documentoRendererService.Setup(x => x.AnexarPdfBase64(DocumentoAssinadoBase64, "pdf")).Returns("assinado+certificado");

            await CriarService().ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, Tracos, "1.1.1.1", "device");

            _contaFiadoService.Verify(x => x.EfetivarTermoAssinadoAsync(assinatura, It.IsAny<DateTime>()), Times.Once);
            _vendaService.Verify(x => x.EfetivarDocumentoAssinadoAsync(It.IsAny<AssinaturaEletronica>(), It.IsAny<DateTime>()), Times.Never);
            assinatura.Assinado.Should().BeTrue();
        }

        [Fact]
        public async Task ObterDocumentoAssinadoAsync_ComercioDiferente_DeveLancarNotFound()
        {
            var venda = Builders.NovaVenda(comercioId: 1);
            var assinatura = Builders.NovaAssinatura();
            assinatura.Venda = venda;
            _assinaturaRepository.Setup(x => x.GetClienteLojaAssinaturaByGuidAsync(It.IsAny<Guid>())).ReturnsAsync(assinatura);

            var service = CriarService();

            var act = async () => await service.ObterDocumentoAssinadoAsync(assinatura.DocumentoGuid, comercioId: 999);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ValidaAssinado_DocumentoInexistente_DeveLancarNotFound()
        {
            _assinaturaRepository.Setup(x => x.GetByGuidAsync(It.IsAny<Guid>())).ReturnsAsync((AssinaturaEletronica?)null);
            var service = CriarService();

            var act = async () => await service.ValidaAssinado(Guid.NewGuid());

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ValidaAssinado_DocumentoAssinado_DeveRetornarTrue()
        {
            var assinatura = Builders.NovaAssinatura(assinado: true);
            _assinaturaRepository.Setup(x => x.GetByGuidAsync(assinatura.DocumentoGuid)).ReturnsAsync(assinatura);

            var service = CriarService();
            var resultado = await service.ValidaAssinado(assinatura.DocumentoGuid);

            resultado.Should().BeTrue();
        }
    }
}
