using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class AutorizacaoCargoServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();

        private AutorizacaoCargoService CriarService() => new(_usuarioRepository.Object);

        [Fact]
        public async Task EhAdminGlobalAsync_UsuarioAdmin_DeveRetornarTrue()
        {
            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1, admin: true));
            var service = CriarService();

            (await service.EhAdminGlobalAsync(1)).Should().BeTrue();
        }

        [Fact]
        public async Task EhAdminGlobalAsync_UsuarioInexistente_DeveRetornarFalse()
        {
            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((ninx.Domain.Entities.Usuario?)null);
            var service = CriarService();

            (await service.EhAdminGlobalAsync(1)).Should().BeFalse();
        }

        [Fact]
        public void GarantirPermissao_AdminGlobal_NuncaLanca()
        {
            var service = CriarService();
            var act = () => service.GarantirPermissao(chamadorEhAdminGlobal: true, chamadorEhProprietario: false, permissoesChamador: [], "GerenciarUsuarios", "erro");
            act.Should().NotThrow();
        }

        [Fact]
        public void GarantirPermissao_Proprietario_NuncaLanca()
        {
            var service = CriarService();
            var act = () => service.GarantirPermissao(chamadorEhAdminGlobal: false, chamadorEhProprietario: true, permissoesChamador: [], "GerenciarUsuarios", "erro");
            act.Should().NotThrow();
        }

        [Fact]
        public void GarantirPermissao_SemAPermissao_DeveLancarForbidden()
        {
            var service = CriarService();
            var act = () => service.GarantirPermissao(chamadorEhAdminGlobal: false, chamadorEhProprietario: false, permissoesChamador: ["VisualizarRelatorios"], "GerenciarUsuarios", "sem permissão");
            act.Should().Throw<ForbiddenException>().WithMessage("sem permissão");
        }

        [Fact]
        public void GarantirPermissao_ComAPermissao_NaoDeveLancar()
        {
            var service = CriarService();
            var act = () => service.GarantirPermissao(chamadorEhAdminGlobal: false, chamadorEhProprietario: false, permissoesChamador: ["GerenciarUsuarios"], "GerenciarUsuarios", "erro");
            act.Should().NotThrow();
        }

        [Fact]
        public void GarantirNaoProprietario_AdminGlobal_NuncaLanca()
        {
            var service = CriarService();
            var act = () => service.GarantirNaoProprietario(chamadorEhAdminGlobal: true, ehProprietario: true, "erro");
            act.Should().NotThrow();
        }

        [Fact]
        public void GarantirNaoProprietario_AlvoProprietario_DeveLancarForbidden()
        {
            var service = CriarService();
            var act = () => service.GarantirNaoProprietario(chamadorEhAdminGlobal: false, ehProprietario: true, "erro");
            act.Should().Throw<ForbiddenException>().WithMessage("erro");
        }

        [Fact]
        public void GarantirNaoProprietario_AlvoComum_NaoDeveLancar()
        {
            var service = CriarService();
            var act = () => service.GarantirNaoProprietario(chamadorEhAdminGlobal: false, ehProprietario: false, "erro");
            act.Should().NotThrow();
        }

        [Fact]
        public void GarantirSemEscalonamento_PermissaoAlemDoChamador_DeveLancarForbidden()
        {
            var service = CriarService();
            var act = () => service.GarantirSemEscalonamento(chamadorEhAdminGlobal: false, chamadorEhProprietario: false,
                permissoesChamador: ["GerenciarCargos"], permissoesRequisitadas: ["GerenciarCargos", "GerenciarComercio"], "erro");
            act.Should().Throw<ForbiddenException>().WithMessage("erro");
        }

        [Fact]
        public void GarantirSemEscalonamento_Proprietario_NuncaLanca()
        {
            var service = CriarService();
            var act = () => service.GarantirSemEscalonamento(chamadorEhAdminGlobal: false, chamadorEhProprietario: true,
                permissoesChamador: [], permissoesRequisitadas: ["GerenciarComercio"], "erro");
            act.Should().NotThrow();
        }
    }

    public class AutorizacaoGlobalServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();

        private AutorizacaoGlobalService CriarService() => new(_usuarioRepository.Object);

        [Fact]
        public async Task GarantirAdministradorGlobalAsync_UsuarioNaoAdmin_DeveLancarUnauthorized()
        {
            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1, admin: false));
            var service = CriarService();

            var act = async () => await service.GarantirAdministradorGlobalAsync(1);

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task GarantirAdministradorGlobalAsync_UsuarioInexistente_DeveLancarUnauthorized()
        {
            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((ninx.Domain.Entities.Usuario?)null);
            var service = CriarService();

            var act = async () => await service.GarantirAdministradorGlobalAsync(1);

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task GarantirAdministradorGlobalAsync_UsuarioAdmin_DeveRetornarUsuario()
        {
            var usuario = Builders.NovoUsuario(1, admin: true);
            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(usuario);
            var service = CriarService();

            var resultado = await service.GarantirAdministradorGlobalAsync(1);

            resultado.Should().Be(usuario);
        }
    }
}
