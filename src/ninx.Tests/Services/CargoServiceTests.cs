using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class CargoServiceTests
    {
        private readonly Mock<ICargoRepository> _cargoRepository = new();
        private readonly Mock<IPermissaoRepository> _permissaoRepository = new();
        private readonly Mock<IUsuarioComercioRepository> _usuarioComercioRepository = new();
        private readonly Mock<IAutorizacaoCargoService> _autorizacaoCargoService = new();
        private readonly Mock<IAutorizacaoGlobalService> _autorizacaoGlobalService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private CargoService CriarService() => new(
            _cargoRepository.Object,
            _permissaoRepository.Object,
            _usuarioComercioRepository.Object,
            _autorizacaoCargoService.Object,
            _autorizacaoGlobalService.Object,
            _unitOfWork.Object);

        [Fact]
        public async Task CriarAsync_CargoBase_UsuarioNaoAdmin_DeveLancarUnauthorized()
        {
            _autorizacaoGlobalService.Setup(x => x.GarantirAdministradorGlobalAsync(1))
                .ThrowsAsync(new UnauthorizedException("Você não possui permissão para utilizar esse endpoint"));

            var service = CriarService();
            var request = new CriarCargoRequest { Nome = "Novo", PermissaoIds = [1], ComercioID = null };

            var act = async () => await service.CriarAsync(request, 1);

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task CriarAsync_CargoBase_NomeJaExistente_DeveLancarBadRequest()
        {
            _autorizacaoGlobalService.Setup(x => x.GarantirAdministradorGlobalAsync(1)).ReturnsAsync(Builders.NovoUsuario(1, admin: true));
            _cargoRepository.Setup(x => x.ExisteNomeAsync("Gerente", null)).ReturnsAsync(true);

            var service = CriarService();
            var request = new CriarCargoRequest { Nome = "Gerente", PermissaoIds = [1], ComercioID = null };

            var act = async () => await service.CriarAsync(request, 1);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_CargoDeComercio_SemVinculoDeDono_DeveLancarForbidden()
        {
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync((UsuarioComercio?)null);
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);

            var service = CriarService();
            var request = new CriarCargoRequest { Nome = "Vendedor", PermissaoIds = [], ComercioID = 1 };

            var act = async () => await service.CriarAsync(request, 1);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CriarAsync_CargoDeComercio_PermissaoAlemDoChamador_DeveLancarForbidden()
        {
            var vinculo = Builders.NovoVinculo(1, 1, Builders.NovoCargo(permissoes: [PermissaoConstantes.GerenciarCargos]));
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync(vinculo);
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _cargoRepository.Setup(x => x.ExisteNomeAsync("Vendedor", 1)).ReturnsAsync(false);
            _permissaoRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync([new Permissao { PermissaoID = 1, Chave = PermissaoConstantes.GerenciarComercio, Nome = "x" }]);
            _autorizacaoCargoService.Setup(x => x.GarantirSemEscalonamento(false, false, It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>(), It.IsAny<string>()))
                .Throws(new ForbiddenException("Você só pode conceder permissões que você mesmo possui."));

            var service = CriarService();
            var request = new CriarCargoRequest { Nome = "Vendedor", PermissaoIds = [1], ComercioID = 1 };

            var act = async () => await service.CriarAsync(request, 1);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CriarAsync_CargoDeComercio_Valido_DeveCriar()
        {
            var vinculo = Builders.NovoVinculo(1, 1, Builders.NovoCargo(ehProprietario: true));
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(1, 1)).ReturnsAsync(vinculo);
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _cargoRepository.Setup(x => x.ExisteNomeAsync("Vendedor", 1)).ReturnsAsync(false);
            _permissaoRepository.Setup(x => x.GetByIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync([]);

            var service = CriarService();
            var request = new CriarCargoRequest { Nome = "Vendedor", PermissaoIds = [], ComercioID = 1 };

            var response = await service.CriarAsync(request, 1);

            response.Nome.Should().Be("Vendedor");
            _cargoRepository.Verify(x => x.AddAsync(It.IsAny<Cargo>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_CargoInexistente_DeveLancarNotFound()
        {
            _cargoRepository.Setup(x => x.GetComPermissoesAsync(1)).ReturnsAsync((Cargo?)null);
            var service = CriarService();

            var act = async () => await service.AtualizarAsync(1, new AtualizarCargoRequest { Nome = "X", PermissaoIds = [] }, 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task AtualizarAsync_CargoReservado_DeveLancarForbidden()
        {
            var cargo = Builders.NovoCargo(1, comercioId: null);
            cargo.Reservado = true;
            _cargoRepository.Setup(x => x.GetComPermissoesAsync(1)).ReturnsAsync(cargo);
            _autorizacaoGlobalService.Setup(x => x.GarantirAdministradorGlobalAsync(1)).ReturnsAsync(Builders.NovoUsuario(1, admin: true));

            var service = CriarService();

            var act = async () => await service.AtualizarAsync(1, new AtualizarCargoRequest { Nome = "X", PermissaoIds = [] }, 1);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task AtualizarAsync_CargoProprietario_DeveLancarForbidden()
        {
            var cargo = Builders.NovoCargo(1, ehProprietario: true);
            _cargoRepository.Setup(x => x.GetComPermissoesAsync(1)).ReturnsAsync(cargo);
            _autorizacaoCargoService.Setup(x => x.GarantirNaoProprietario(false, true, It.IsAny<string>()))
                .Throws(new ForbiddenException("O cargo de proprietário não pode ser alterado."));

            var service = CriarService();

            var act = async () => await service.AtualizarAsync(1, new AtualizarCargoRequest { Nome = "X", PermissaoIds = [] }, 1);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task DesativarAsync_CargoInexistente_DeveLancarNotFound()
        {
            _cargoRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Cargo?)null);
            var service = CriarService();

            var act = async () => await service.DesativarAsync(1, 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
