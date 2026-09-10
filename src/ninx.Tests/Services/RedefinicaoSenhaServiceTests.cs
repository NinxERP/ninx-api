using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class RedefinicaoSenhaServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
        private readonly Mock<IRedefinicaoSenhaRepository> _redefinicaoSenhaRepository = new();
        private readonly Mock<IEmailService> _emailService = new();
        private readonly Mock<ILogAuditoriaService> _logAuditoriaService = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private RedefinicaoSenhaService CriarService() => new(
            _usuarioRepository.Object,
            _redefinicaoSenhaRepository.Object,
            _emailService.Object,
            _logAuditoriaService.Object,
            _unitOfWork.Object);

        [Fact]
        public async Task SolicitarAsync_UsuarioInexistente_NaoDeveLancarNemEnviarEmail()
        {
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
            var service = CriarService();

            await service.SolicitarAsync(new SolicitarRedefinicaoSenhaRequest { Email = "ninguem@teste.com" });

            _emailService.Verify(x => x.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _redefinicaoSenhaRepository.Verify(x => x.AddAsync(It.IsAny<RedefinicaoSenha>()), Times.Never);
        }

        [Fact]
        public async Task SolicitarAsync_DentroDoCooldown_NaoDeveGerarNovoCodigo()
        {
            var usuario = Builders.NovoUsuario(1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _redefinicaoSenhaRepository.Setup(x => x.GetUltimoAtivoAsync(1)).ReturnsAsync(new RedefinicaoSenha
            {
                UsuarioID = 1,
                CriadoEm = DateTime.UtcNow,
                CodigoHash = "hash",
                ExpiraEm = DateTime.UtcNow.AddMinutes(15)
            });

            var service = CriarService();
            await service.SolicitarAsync(new SolicitarRedefinicaoSenhaRequest { Email = usuario.Email });

            _redefinicaoSenhaRepository.Verify(x => x.AddAsync(It.IsAny<RedefinicaoSenha>()), Times.Never);
        }

        [Fact]
        public async Task SolicitarAsync_Valido_DeveGerarCodigoEEnviarEmail()
        {
            var usuario = Builders.NovoUsuario(1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _redefinicaoSenhaRepository.Setup(x => x.GetUltimoAtivoAsync(1)).ReturnsAsync((RedefinicaoSenha?)null);

            var service = CriarService();
            await service.SolicitarAsync(new SolicitarRedefinicaoSenhaRequest { Email = usuario.Email });

            _redefinicaoSenhaRepository.Verify(x => x.AddAsync(It.IsAny<RedefinicaoSenha>()), Times.Once);
            _emailService.Verify(x => x.EnviarAsync(usuario.Email, usuario.Nome, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task SolicitarAsync_FalhaNoEnvioDeEmail_NaoDevePropagarExcecao()
        {
            var usuario = Builders.NovoUsuario(1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _redefinicaoSenhaRepository.Setup(x => x.GetUltimoAtivoAsync(1)).ReturnsAsync((RedefinicaoSenha?)null);
            _emailService.Setup(x => x.EnviarAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Falha no provedor de email"));

            var service = CriarService();
            var act = async () => await service.SolicitarAsync(new SolicitarRedefinicaoSenhaRequest { Email = usuario.Email });

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task ConfirmarAsync_UsuarioInexistente_DeveLancarBadRequest()
        {
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(It.IsAny<string>())).ReturnsAsync((Usuario?)null);
            var service = CriarService();

            var act = async () => await service.ConfirmarAsync(new ConfirmarRedefinicaoSenhaRequest { Email = "x@x.com", Codigo = "123456", NovaSenha = "novaSenha1" });

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ConfirmarAsync_SemSolicitacaoAtiva_DeveLancarBadRequest()
        {
            var usuario = Builders.NovoUsuario(1);
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _redefinicaoSenhaRepository.Setup(x => x.GetUltimoAtivoAsync(1)).ReturnsAsync((RedefinicaoSenha?)null);

            var service = CriarService();

            var act = async () => await service.ConfirmarAsync(new ConfirmarRedefinicaoSenhaRequest { Email = usuario.Email, Codigo = "123456", NovaSenha = "novaSenha1" });

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ConfirmarAsync_CodigoIncorreto_DeveIncrementarTentativasELancarBadRequest()
        {
            var usuario = Builders.NovoUsuario(1);
            var redefinicao = new RedefinicaoSenha
            {
                UsuarioID = 1,
                CodigoHash = BCrypt.Net.BCrypt.HashPassword("654321"),
                Tentativas = 0,
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddMinutes(10)
            };
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _redefinicaoSenhaRepository.Setup(x => x.GetUltimoAtivoAsync(1)).ReturnsAsync(redefinicao);

            var service = CriarService();

            var act = async () => await service.ConfirmarAsync(new ConfirmarRedefinicaoSenhaRequest { Email = usuario.Email, Codigo = "123456", NovaSenha = "novaSenha1" });

            await act.Should().ThrowAsync<BadRequestException>();
            redefinicao.Tentativas.Should().Be(1);
        }

        [Fact]
        public async Task ConfirmarAsync_ExcedeuMaximoDeTentativas_DeveInvalidarEDeveLancarBadRequest()
        {
            var usuario = Builders.NovoUsuario(1);
            var redefinicao = new RedefinicaoSenha
            {
                UsuarioID = 1,
                CodigoHash = BCrypt.Net.BCrypt.HashPassword("654321"),
                Tentativas = 5,
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddMinutes(10)
            };
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _redefinicaoSenhaRepository.Setup(x => x.GetUltimoAtivoAsync(1)).ReturnsAsync(redefinicao);

            var service = CriarService();

            var act = async () => await service.ConfirmarAsync(new ConfirmarRedefinicaoSenhaRequest { Email = usuario.Email, Codigo = "654321", NovaSenha = "novaSenha1" });

            await act.Should().ThrowAsync<BadRequestException>();
            redefinicao.Utilizado.Should().BeTrue();
        }

        [Fact]
        public async Task ConfirmarAsync_CodigoValido_DeveAtualizarSenha()
        {
            var usuario = Builders.NovoUsuario(1);
            var senhaAntiga = usuario.SenhaHash;
            var redefinicao = new RedefinicaoSenha
            {
                UsuarioID = 1,
                CodigoHash = BCrypt.Net.BCrypt.HashPassword("654321"),
                Tentativas = 0,
                CriadoEm = DateTime.UtcNow,
                ExpiraEm = DateTime.UtcNow.AddMinutes(10)
            };
            _usuarioRepository.Setup(x => x.GetUsuarioByEmail(usuario.Email)).ReturnsAsync(usuario);
            _redefinicaoSenhaRepository.Setup(x => x.GetUltimoAtivoAsync(1)).ReturnsAsync(redefinicao);

            var service = CriarService();
            await service.ConfirmarAsync(new ConfirmarRedefinicaoSenhaRequest { Email = usuario.Email, Codigo = "654321", NovaSenha = "novaSenha1" });

            usuario.SenhaHash.Should().NotBe(senhaAntiga);
            BCrypt.Net.BCrypt.Verify("novaSenha1", usuario.SenhaHash).Should().BeTrue();
            redefinicao.Utilizado.Should().BeTrue();
        }
    }
}
