using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class PagamentoHistoricoAssinaturaPlanoServiceTests
    {
        private readonly Mock<IPagamentoHistoricoAssinaturaPlanoRepository> _pagamentoRepository = new();
        private readonly Mock<IAssinaturaPlanoRepository> _assinaturaPlanoRepository = new();
        private readonly Mock<IAutorizacaoGlobalService> _autorizacaoGlobalService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private PagamentoHistoricoAssinaturaPlanoService CriarService() => new(
            _pagamentoRepository.Object,
            _assinaturaPlanoRepository.Object,
            _autorizacaoGlobalService.Object,
            _unitOfWork.Object,
            Mock.Of<Microsoft.Extensions.Logging.ILogger<PagamentoHistoricoAssinaturaPlanoService>>());

        [Fact]
        public async Task RegistrarPagamentos_ComCancelamentoPendente_DeveLimparCancelamentoSolicitado()
        {
            var assinatura = Builders.NovaAssinaturaPlano(1, cancelamentoSolicitadoEm: DateTime.UtcNow.AddDays(-1));
            _autorizacaoGlobalService.Setup(x => x.GarantirAdministradorGlobalAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync(assinatura);

            var service = CriarService();
            await service.RegistrarPagamentos(new PagamentoHistoricoAssinaturaPlanoRequest { ComercioId = 1, Valor = 50 }, usuarioLogadoId: 1);

            assinatura.CancelamentoSolicitadoEm.Should().BeNull();
            assinatura.Status.Should().Be(StatusAssinatura.Ativa);
            _assinaturaPlanoRepository.Verify(x => x.UpdateAsync(assinatura), Times.Once);
        }
    }
}
