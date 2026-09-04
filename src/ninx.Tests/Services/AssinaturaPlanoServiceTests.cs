using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Domain.Entities;
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

        private AssinaturaPlanoService CriarService() => new(_assinaturaPlanoRepository.Object);

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
    }
}
