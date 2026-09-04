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

        private AssinaturaEletronicaService CriarService() => new(
            _assinaturaRepository.Object,
            _unitOfWork.Object,
            _vendaRepository.Object,
            _documentoRendererService.Object);

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
            _vendaRepository.Setup(x => x.GetByIdAsync(venda.VendaID)).ReturnsAsync(venda);

            var service = CriarService();

            var act = async () => await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, "img", "1.1.1.1", "device");

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ConfirmarAssinaturaAsync_Valida_DeveMarcarComoAssinadaEReabrirVenda()
        {
            var venda = Builders.NovaVenda(status: StatusVenda.Aguardando);
            var assinatura = Builders.NovaAssinatura(vendaId: venda.VendaID);
            _assinaturaRepository.Setup(x => x.GetAllByGuidAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<AssinaturaEletronica> { assinatura });
            _vendaRepository.Setup(x => x.GetByIdAsync(venda.VendaID)).ReturnsAsync(venda);
            _documentoRendererService.Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>())).ReturnsAsync("pdf");

            var service = CriarService();
            await service.ConfirmarAssinaturaAsync(assinatura.DocumentoGuid, "imgBase64", "1.1.1.1", "device");

            assinatura.Assinado.Should().BeTrue();
            assinatura.ImagemAssinatura.Should().Be("imgBase64");
            venda.Status.Should().Be(StatusVenda.Aberta);
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
            _vendaRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(venda1);
            _vendaRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(venda2);
            _documentoRendererService.Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>())).ReturnsAsync("pdf");

            var service = CriarService();
            await service.ConfirmarAssinaturaAsync(guid, "imgBase64", "1.1.1.1", "device");

            assinatura1.Assinado.Should().BeTrue();
            assinatura2.Assinado.Should().BeTrue();
            _assinaturaRepository.Verify(x => x.UpdateAsync(It.IsAny<AssinaturaEletronica>()), Times.Exactly(2));
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
