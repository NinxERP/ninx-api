using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class LoginServiceTests
    {
        private readonly Mock<ITokenProvider> _tokenProvider = new();
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
        private readonly Mock<IUsuarioComercioRepository> _usuarioComercioRepository = new();
        private readonly Mock<IAssinaturaPlanoRepository> _assinaturaPlanoRepository = new();
        private readonly Mock<ICargoEfetivoService> _cargoEfetivoService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private LoginService CriarService() => new(
            _tokenProvider.Object,
            _usuarioRepository.Object,
            _usuarioComercioRepository.Object,
            _assinaturaPlanoRepository.Object,
            _cargoEfetivoService.Object,
            _unitOfWork.Object);

        private const string SenhaValida = "senha123";

        [Fact]
        public async Task LoginAsync_EmailInexistente_DeveLancarBadRequest()
        {
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
            var service = CriarService();

            var act = async () => await service.LoginAsync(new LoginRequest { Email = "a@a.com", Senha = "x" });

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task LoginAsync_SenhaIncorreta_DeveLancarBadRequest()
        {
            var usuario = Builders.NovoUsuario(1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            var service = CriarService();

            var act = async () => await service.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = "senhaErrada" });

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task LoginAsync_UsuarioInativo_DeveLancarForbidden()
        {
            var usuario = Builders.NovoUsuario(1, ativo: false);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            var service = CriarService();

            var act = async () => await service.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = SenhaValida });

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task LoginAsync_SemComerciosVinculados_DeveLancarForbidden()
        {
            var usuario = Builders.NovoUsuario(1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _usuarioComercioRepository.Setup(x => x.GetByUsuarioIdAsync(1)).ReturnsAsync(new List<UsuarioComercio>());
            var service = CriarService();

            var act = async () => await service.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = SenhaValida });

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task LoginAsync_UnicoComercio_DeveEmitirTokenDireto()
        {
            var usuario = Builders.NovoUsuario(1);
            var vinculo = Builders.NovoVinculo(1, 1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _usuarioComercioRepository.Setup(x => x.GetByUsuarioIdAsync(1)).ReturnsAsync(new List<UsuarioComercio> { vinculo });
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync(Builders.NovaAssinaturaPlano(1));
            _cargoEfetivoService.Setup(x => x.ResolverCargoEfetivoAsync(usuario, vinculo.Cargo)).ReturnsAsync(vinculo.Cargo);
            _tokenProvider.Setup(x => x.GerarToken(usuario, 1, vinculo.Cargo, vinculo.Comercio.NomeComercio)).Returns("token-jwt");

            var service = CriarService();
            var response = await service.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = SenhaValida });

            response.Token.Should().Be("token-jwt");
        }

        [Fact]
        public async Task LoginAsync_MultiplosComerciosSemSelecao_DeveRetornarListaDeComercios()
        {
            var usuario = Builders.NovoUsuario(1);
            var vinculo1 = Builders.NovoVinculo(1, 1);
            var vinculo2 = Builders.NovoVinculo(1, 2);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _usuarioComercioRepository.Setup(x => x.GetByUsuarioIdAsync(1)).ReturnsAsync(new List<UsuarioComercio> { vinculo1, vinculo2 });

            var service = CriarService();
            var response = await service.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = SenhaValida });

            response.Token.Should().BeNull();
            response.Comercios.Should().HaveCount(2);
        }

        [Fact]
        public async Task LoginAsync_ComercioSelecionadoSemVinculo_DeveLancarUnauthorized()
        {
            var usuario = Builders.NovoUsuario(1);
            var vinculo = Builders.NovoVinculo(1, 1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _usuarioComercioRepository.Setup(x => x.GetByUsuarioIdAsync(1)).ReturnsAsync(new List<UsuarioComercio> { vinculo });

            var service = CriarService();

            var act = async () => await service.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = SenhaValida, ComercioID = 999 });

            await act.Should().ThrowAsync<UnauthorizedException>();
        }

        [Fact]
        public async Task LoginAsync_AssinaturaVencida_DeveLancarForbiddenEAtualizarStatus()
        {
            var usuario = Builders.NovoUsuario(1);
            var vinculo = Builders.NovoVinculo(1, 1);
            var assinatura = Builders.NovaAssinaturaPlano(1, dataFim: DateTime.UtcNow.AddDays(-1));

            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _usuarioComercioRepository.Setup(x => x.GetByUsuarioIdAsync(1)).ReturnsAsync(new List<UsuarioComercio> { vinculo });
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync(assinatura);

            var service = CriarService();

            var act = async () => await service.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = SenhaValida });

            await act.Should().ThrowAsync<ForbiddenException>();
            assinatura.Status.Should().Be(StatusAssinatura.Vencida);
            _assinaturaPlanoRepository.Verify(x => x.UpdateAsync(assinatura), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ComercioSemPlano_DeveLancarNotFound()
        {
            var usuario = Builders.NovoUsuario(1);
            var vinculo = Builders.NovoVinculo(1, 1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _usuarioComercioRepository.Setup(x => x.GetByUsuarioIdAsync(1)).ReturnsAsync(new List<UsuarioComercio> { vinculo });
            _assinaturaPlanoRepository.Setup(x => x.GetByComercioIdAsync(1)).ReturnsAsync((AssinaturaPlano?)null);

            var service = CriarService();

            var act = async () => await service.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = SenhaValida });

            await act.Should().ThrowAsync<NotFoundException>();
        }
    }
}
