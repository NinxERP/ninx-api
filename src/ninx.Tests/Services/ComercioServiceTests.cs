using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class ComercioServiceTests
    {
        private readonly Mock<IComercioRepository> _comercioRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IUsuarioComercioRepository> _usuarioComercioRepository = new();
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
        private readonly Mock<IAssinaturaPlanoRepository> _assinaturaPlanoRepository = new();
        private readonly Mock<IPagamentoHistoricoAssinaturaPlanoRepository> _pagamentoHistoricoRepository = new();
        private readonly Mock<IAutorizacaoCargoService> _autorizacaoCargoService = new();
        private readonly Mock<IAutorizacaoGlobalService> _autorizacaoGlobalService = new();
        private readonly Mock<ILogAuditoriaService> _logAuditoriaService = new();

        private ComercioService CriarService() => new(
            _comercioRepository.Object,
            _unitOfWork.Object,
            _usuarioComercioRepository.Object,
            _usuarioRepository.Object,
            _assinaturaPlanoRepository.Object,
            _pagamentoHistoricoRepository.Object,
            _autorizacaoCargoService.Object,
            _autorizacaoGlobalService.Object,
            _logAuditoriaService.Object);

        [Fact]
        public async Task GetByIdAsync_ComercioInexistente_DeveLancarNotFound()
        {
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Comercio?)null);
            var service = CriarService();

            var act = async () => await service.GetByIdAsync(1, usuarioIdLogado: 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetByIdAsync_UsuarioSemVinculoENaoAdmin_DeveLancarNotFound()
        {
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync((UsuarioComercio?)null);

            var service = CriarService();

            var act = async () => await service.GetByIdAsync(1, usuarioIdLogado: 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetByUsuarioId_UsuarioDiferenteDoLogado_DeveLancarUnauthorized()
        {
            var service = CriarService();

            var act = async () => await service.GetByUsuarioId(usuarioId: 2, usuarioIdLogado: 1);

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CriarAsync_DeveCriarComercioComAssinaturaMensalGratuita()
        {
            _autorizacaoGlobalService.Setup(x => x.GarantirAdministradorGlobalAsync(1)).ReturnsAsync(Builders.NovoUsuario(1, admin: true));
            var service = CriarService();

            var response = await service.CriarAsync(new ComercioRequest { Nome = "Novo Comércio" }, usuarioIdLogado: 1);

            response.Should().NotBeNull();
            _assinaturaPlanoRepository.Verify(x => x.AddAsync(It.IsAny<AssinaturaPlano>()), Times.Once);
            _pagamentoHistoricoRepository.Verify(x => x.AddAsync(It.IsAny<PagamentoHistoricoAssinaturaPlano>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_ComercioInexistente_DeveLancarNotFound()
        {
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Comercio?)null);
            var service = CriarService();

            var act = async () => await service.AtualizarAsync(1, 1, new ComercioRequest { Nome = "X" });

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task AtualizarAsync_UsuarioSemPermissaoDeGerenciarComercio_DeveLancarForbidden()
        {
            var comercio = Builders.NovoComercio(1);
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(comercio);
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync(Builders.NovoVinculo(1, 1, Builders.NovoCargo()));
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _autorizacaoCargoService.Setup(x => x.GarantirPermissao(false, false, It.IsAny<IEnumerable<string>>(), PermissaoConstantes.GerenciarComercio, It.IsAny<string>()))
                .Throws(new ForbiddenException("Acesso negado."));

            var service = CriarService();

            var act = async () => await service.AtualizarAsync(1, 1, new ComercioRequest { Nome = "X" });

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task DesativarAsync_ComercioValido_DeveDesativarERegistrarAuditoria()
        {
            var comercio = Builders.NovoComercio(1);
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(comercio);
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync(Builders.NovoVinculo(1, 1, Builders.NovoCargo(ehProprietario: true)));
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);

            var service = CriarService();
            await service.DesativarAsync(1, 1);

            comercio.Ativo.Should().BeFalse();
            _logAuditoriaService.Verify(x => x.RegistrarAsync(1, 1, "ComercioDesativado", "Comercio", 1, null), Times.Once);
        }
    }
}
