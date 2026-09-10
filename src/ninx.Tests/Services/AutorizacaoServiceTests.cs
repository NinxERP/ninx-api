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
        public void GarantirGerencia_AdminGlobal_NuncaLanca()
        {
            var service = CriarService();
            var act = () => service.GarantirGerencia(chamadorEhAdminGlobal: true, pesoChamador: 1, pesoAlvo: 100);
            act.Should().NotThrow();
        }

        [Fact]
        public void GarantirGerencia_PesoMenorOuIgualAoAlvo_DeveLancarForbidden()
        {
            var service = CriarService();
            var act = () => service.GarantirGerencia(chamadorEhAdminGlobal: false, pesoChamador: 20, pesoAlvo: 20);
            act.Should().Throw<ForbiddenException>();
        }

        [Fact]
        public void GarantirGerencia_PesoMaiorQueAlvo_NaoDeveLancar()
        {
            var service = CriarService();
            var act = () => service.GarantirGerencia(chamadorEhAdminGlobal: false, pesoChamador: 30, pesoAlvo: 20);
            act.Should().NotThrow();
        }

        [Fact]
        public void GarantirPesoMinimo_AbaixoDoMinimo_DeveLancarForbidden()
        {
            var service = CriarService();
            var act = () => service.GarantirPesoMinimo(chamadorEhAdminGlobal: false, pesoChamador: 5, pesoMinimo: 20, "sem permissão");
            act.Should().Throw<ForbiddenException>().WithMessage("sem permissão");
        }

        [Fact]
        public void GarantirPesoMinimo_AdminGlobal_NuncaLanca()
        {
            var service = CriarService();
            var act = () => service.GarantirPesoMinimo(chamadorEhAdminGlobal: true, pesoChamador: 0, pesoMinimo: 999, "erro");
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
