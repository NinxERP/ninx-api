using FluentAssertions;
using Moq;
using ninx.Application.Services;
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

        // A confirmação recebe o PDF assinado em base64 (validado pelo request validator).
        private static readonly string DocumentoAssinadoBase64 = Convert.ToBase64String("%PDF-1.7 documento assinado"u8.ToArray());

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

            var act = async () => await service.ConfirmarAssinaturaAsync(Guid.NewGuid(), "img", "1.1.1.1", "device");

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_JaAssinado_DeveLancarBadRequest()
        {
            var assinatura = Builders.NovaAssinatura(assinado: true);
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });

            var service = CriarService();

            var act = async () => await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, "img", "1.1.1.1", "device");

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_StatusNaoAtivo_DeveLancarBadRequest()
        {
            var assinatura = Builders.NovaAssinatura(status: StatusAssinatura.Cancelada);
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });

            var service = CriarService();

            var act = async () => await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, "img", "1.1.1.1", "device");

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

            var act = async () => await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, DocumentoAssinadoBase64, "1.1.1.1", "device");

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
            await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, DocumentoAssinadoBase64, "1.1.1.1", "device");

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
            await service.ConfirmarAssinaturaAsync(guid, DocumentoAssinadoBase64, "1.1.1.1", "device");

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

            await CriarService().ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, DocumentoAssinadoBase64, "1.1.1.1", "device");

            _contaFiadoService.Verify(x => x.EfetivarTermoAssinadoAsync(assinatura, It.IsAny<DateTime>()), Times.Once);
            _vendaService.Verify(x => x.EfetivarDocumentoAssinadoAsync(It.IsAny<AssinaturaEletronica>(), It.IsAny<DateTime>()), Times.Never);
            assinatura.Assinado.Should().BeTrue();
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_Base64Invalido_DeveLancarBadRequest()
        {
            var assinatura = Builders.NovaAssinatura();
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });

            var act = async () => await CriarService().ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, "isto nao e base64!", "1.1.1.1", "device");

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_PdfQueNaoAbre_DeveLancarBadRequest()
        {
            var assinatura = Builders.NovaAssinatura();
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });
            _documentoRendererService.Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>())).ReturnsAsync("pdf");
            _documentoRendererService.Setup(x => x.AnexarPdfBase64(It.IsAny<string>(), It.IsAny<string>())).Throws(new InvalidOperationException("PDF corrompido"));

            var act = async () => await CriarService().ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, DocumentoAssinadoBase64, "1.1.1.1", "device");

            await act.Should().ThrowAsync<BadRequestException>();
            _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Never);
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
