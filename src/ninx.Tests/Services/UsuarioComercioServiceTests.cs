using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class UsuarioComercioServiceTests
    {
        private readonly Mock<IUsuarioComercioRepository> _usuarioComercioRepository = new();
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
        private readonly Mock<ICargoRepository> _cargoRepository = new();
        private readonly Mock<IAutorizacaoCargoService> _autorizacaoCargoService = new();
        private readonly Mock<ILogAuditoriaService> _logAuditoriaService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private UsuarioComercioService CriarService() => new(
            _usuarioComercioRepository.Object,
            _usuarioRepository.Object,
            _cargoRepository.Object,
            _autorizacaoCargoService.Object,
            _logAuditoriaService.Object,
            _unitOfWork.Object);

        [Fact]
        public async Task CriarAsync_ChamadorSemPermissao_DeveLancarForbidden()
        {
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync((UsuarioComercio?)null);
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _autorizacaoCargoService.Setup(x => x.GarantirPermissao(false, false, It.IsAny<IEnumerable<string>>(), It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new ForbiddenException("Você não tem permissão para vincular usuários a este comércio."));

            var service = CriarService();
            var request = new CriarUsuarioComercioRequest { UsuarioID = 2, ComercioID = 1, CargoID = 1 };

            var act = async () => await service.CriarAsync(request, usuarioLogadoId: 1);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CriarAsync_CargoInvalido_DeveLancarBadRequest()
        {
            var vinculoDono = Builders.NovoVinculo(1, 1, Builders.NovoCargo(ehProprietario: true));
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync(vinculoDono);
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _cargoRepository.Setup(x => x.GetByIdAsync(99)).ReturnsAsync((Cargo?)null);

            var service = CriarService();
            var request = new CriarUsuarioComercioRequest { UsuarioID = 2, ComercioID = 1, CargoID = 99 };

            var act = async () => await service.CriarAsync(request, usuarioLogadoId: 1);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_UsuarioJaVinculado_DeveLancarBadRequest()
        {
            var vinculoDono = Builders.NovoVinculo(1, 1, Builders.NovoCargo(ehProprietario: true));
            var cargoAlvo = Builders.NovoCargo(2, comercioId: 1);
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync(vinculoDono);
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _cargoRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(cargoAlvo);
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(2, 1)).ReturnsAsync(true);

            var service = CriarService();
            var request = new CriarUsuarioComercioRequest { UsuarioID = 2, ComercioID = 1, CargoID = 2 };

            var act = async () => await service.CriarAsync(request, usuarioLogadoId: 1);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_Valido_DeveVincularUsuario()
        {
            var vinculoDono = Builders.NovoVinculo(1, 1, Builders.NovoCargo(ehProprietario: true));
            var cargoAlvo = Builders.NovoCargo(2, comercioId: 1);
            var usuarioAlvo = Builders.NovoUsuario(2);

            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync(vinculoDono);
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _cargoRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(cargoAlvo);
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(2, 1)).ReturnsAsync(false);
            _usuarioRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(usuarioAlvo);

            var service = CriarService();
            var request = new CriarUsuarioComercioRequest { UsuarioID = 2, ComercioID = 1, CargoID = 2 };

            var response = await service.CriarAsync(request, usuarioLogadoId: 1);

            response.Should().NotBeNull();
            _usuarioComercioRepository.Verify(x => x.AddAsync(It.IsAny<UsuarioComercio>()), Times.Once);
        }

        [Fact]
        public async Task DesativarAsync_VinculoInexistente_DeveLancarNotFound()
        {
            var vinculoDono = Builders.NovoVinculo(1, 1, Builders.NovoCargo(ehProprietario: true));
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync(vinculoDono);
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(2, 1)).ReturnsAsync((UsuarioComercio?)null);

            var service = CriarService();

            var act = async () => await service.DesativarAsync(usuarioId: 2, comercioId: 1, usuarioLogadoId: 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
