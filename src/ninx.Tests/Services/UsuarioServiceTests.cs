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
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
        private readonly Mock<IUsuarioComercioRepository> _usuarioComercioRepository = new();
        private readonly Mock<IUsuarioComercioService> _usuarioComercioService = new();
        private readonly Mock<ICargoRepository> _cargoRepository = new();
        private readonly Mock<IAutorizacaoCargoService> _autorizacaoCargoService = new();
        private readonly Mock<IAutorizacaoGlobalService> _autorizacaoGlobalService = new();
        private readonly Mock<ILogAuditoriaService> _logAuditoriaService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private UsuarioService CriarService() => new(
            _usuarioRepository.Object,
            _usuarioComercioRepository.Object,
            _usuarioComercioService.Object,
            _cargoRepository.Object,
            _autorizacaoCargoService.Object,
            _autorizacaoGlobalService.Object,
            _logAuditoriaService.Object,
            _unitOfWork.Object);

        [Fact]
        public async Task GetById_NaoAdmin_DeveLancarUnauthorized()
        {
            _autorizacaoGlobalService.Setup(x => x.GarantirAdministradorGlobalAsync(1))
                .ThrowsAsync(new UnauthorizedException("Você não possui permissão para utilizar esse endpoint"));

            var service = CriarService();
            var act = async () => await service.GetById(2, 1);

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task GetById_UsuarioInexistente_DeveLancarNotFound()
        {
            _autorizacaoGlobalService.Setup(x => x.GarantirAdministradorGlobalAsync(1)).ReturnsAsync(Builders.NovoUsuario(1, admin: true));
            _usuarioRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync((Usuario?)null);

            var service = CriarService();
            var act = async () => await service.GetById(2, 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CriarAsync_ContextoDeComercioInvalido_DeveLancarBadRequest()
        {
            var service = CriarService();
            var request = new CriarUsuarioRequest { Nome = "X", Email = "x@x.com", Senha = "senha123", CargoID = 1, ComercioId = 1 };

            var act = async () => await service.CriarAsync(request, executorId: 1, ehProprietarioLogado: true, permissoesLogado: [], comercioIdLogado: 2);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_EmailJaCadastrado_DeveLancarBadRequest()
        {
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _cargoRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoCargo(1, comercioId: 1));
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail("existente@teste.com")).ReturnsAsync(Builders.NovoUsuario(2));

            var service = CriarService();
            var request = new CriarUsuarioRequest { Nome = "X", Email = "existente@teste.com", Senha = "senha123", CargoID = 1, ComercioId = 1 };

            var act = async () => await service.CriarAsync(request, executorId: 1, ehProprietarioLogado: true, permissoesLogado: [], comercioIdLogado: 1);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_CargoDeProprietario_DeveLancarForbidden()
        {
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _cargoRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoCargo(1, ehProprietario: true, comercioId: 1));
            _autorizacaoCargoService.Setup(x => x.GarantirNaoProprietario(false, true, It.IsAny<string>()))
                .Throws(new ForbiddenException("Você não pode cadastrar usuários com o cargo de proprietário."));

            var service = CriarService();
            var request = new CriarUsuarioRequest { Nome = "X", Email = "novo@teste.com", Senha = "senha123", CargoID = 1, ComercioId = 1 };

            var act = async () => await service.CriarAsync(request, executorId: 1, ehProprietarioLogado: true, permissoesLogado: [], comercioIdLogado: 1);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task CriarAsync_Valido_DeveCriarUsuarioEVincular()
        {
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _cargoRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoCargo(1, comercioId: 1));
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail("novo@teste.com")).ReturnsAsync((Usuario?)null);

            var service = CriarService();
            var request = new CriarUsuarioRequest { Nome = "Novo", Email = "novo@teste.com", Senha = "senha123", CargoID = 1, ComercioId = 1 };

            var response = await service.CriarAsync(request, executorId: 1, ehProprietarioLogado: true, permissoesLogado: [], comercioIdLogado: 1);

            response.Should().NotBeNull();
            _usuarioRepository.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Once);
            _usuarioComercioRepository.Verify(x => x.AddAsync(It.IsAny<UsuarioComercio>()), Times.Once);
        }

        [Fact]
        public async Task AtualizarAsync_UsuarioNaoPertenceAoComercio_DeveLancarUnauthorized()
        {
            _autorizacaoCargoService.Setup(x => x.EhAdminGlobalAsync(1)).ReturnsAsync(false);
            _usuarioRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(Builders.NovoUsuario(2));
            _usuarioComercioRepository.Setup(x => x.GetVinculoAsync(2, 1)).ReturnsAsync((UsuarioComercio?)null);

            var service = CriarService();
            var request = new AtualizarUsuarioRequest { Nome = "X", Email = "x@x.com" };

            var act = async () => await service.AtualizarAsync(2, request, comercioId: 1, usuarioIdLogado: 1, ehProprietarioLogado: true, permissoesLogado: []);

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task ResetarSenhaAsync_NaoAdmin_DeveLancarUnauthorized()
        {
            _autorizacaoGlobalService.Setup(x => x.GarantirAdministradorGlobalAsync(1))
                .ThrowsAsync(new UnauthorizedException("Você não possui permissão para utilizar esse endpoint"));

            var service = CriarService();
            var act = async () => await service.ResetarSenhaAsync(2, 1, new ResetarSenhaRequest { NovaSenha = "novaSenha1" });

            await act.Should().ThrowAsync<UnauthorizedException>();
        }
    }
}
