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
    /// <summary>
    /// Cobertura leve do AssinaturaPlanoService — serviço fino, majoritariamente delegando ao repositório.
    /// </summary>
    public class AssinaturaPlanoServiceTests
    {
        private readonly Mock<IAssinaturaPlanoRepository> _assinaturaPlanoRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private AssinaturaPlanoService CriarService() => new(_assinaturaPlanoRepository.Object, _unitOfWork.Object);

        [Fact]
        public async Task GetByIdAsync_Inexistente_DeveLancarNotFound()
        {
            _assinaturaPlanoRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((AssinaturaPlano?)null);
            var service = CriarService();

            var act = async () => await service.GetByIdAsync(1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetByComercioIdAsync_PesoInsuficiente_DeveLancarForbidden()
        {
            var service = CriarService();

            var act = async () => await service.GetByComercioIdAsync(1, pesoLogado: 5);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task GetByComercioIdAsync_SemAssinatura_DeveLancarNotFound()
        {
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync((AssinaturaPlano?)null);
            var service = CriarService();

            var act = async () => await service.GetByComercioIdAsync(1, pesoLogado: 20);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetByComercioIdAsync_Valido_DeveRetornarAssinatura()
        {
            var assinatura = Builders.NovaAssinaturaPlano(1);
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync(assinatura);
            var service = CriarService();

            var response = await service.GetByComercioIdAsync(1, pesoLogado: 20);

            response.ComercioID.Should().Be(1);
        }

        [Fact]
        public async Task CancelarAsync_PesoInsuficiente_DeveLancarForbidden()
        {
            var service = CriarService();

            var act = async () => await service.CancelarAsync(1, pesoLogado: 5);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CancelarAsync_SemAssinatura_DeveLancarNotFound()
        {
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync((AssinaturaPlano?)null);
            var service = CriarService();

            var act = async () => await service.CancelarAsync(1, pesoLogado: 20);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CancelarAsync_StatusNaoAtiva_DeveLancarBadRequest()
        {
            var assinatura = Builders.NovaAssinaturaPlano(1, status: StatusAssinatura.Vencida);
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync(assinatura);
            var service = CriarService();

            var act = async () => await service.CancelarAsync(1, pesoLogado: 20);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CancelarAsync_JaTemCancelamentoPendente_DeveLancarBadRequest()
        {
            var assinatura = Builders.NovaAssinaturaPlano(1, cancelamentoSolicitadoEm: DateTime.UtcNow.AddDays(-1));
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync(assinatura);
            var service = CriarService();

            var act = async () => await service.CancelarAsync(1, pesoLogado: 20);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CancelarAsync_Valido_DeveGravarCancelamentoSolicitadoEmESalvar()
        {
            var assinatura = Builders.NovaAssinaturaPlano(1);
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync(assinatura);
            var service = CriarService();

            await service.CancelarAsync(1, pesoLogado: 20);

            assinatura.CancelamentoSolicitadoEm.Should().NotBeNull();
            assinatura.Status.Should().Be(StatusAssinatura.Ativa);
            _assinaturaPlanoRepository.Verify(x => x.UpdateAsync(assinatura), Times.Once);
            _unitOfWork.Verify(x => x.SaveChangesAsync(), Times.Once);
        }
    }
}
