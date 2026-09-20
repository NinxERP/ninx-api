using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    public class ContaFiadoServiceTests
    {
        private readonly Mock<IClienteRepository> _clienteRepository = new();
        private readonly Mock<IComercioRepository> _comercioRepository = new();
        private readonly Mock<IPessoaAutorizadaRepository> _pessoaRepository = new();
        private readonly Mock<ITermoAberturaContaRepository> _termoRepository = new();
        private readonly Mock<IAssinaturaEletronicaRepository> _assinaturaRepository = new();
        private readonly Mock<IVendaRepository> _vendaRepository = new();
        private readonly Mock<IDocumentoRendererService> _renderer = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        public ContaFiadoServiceTests()
        {
            _clienteRepository.Setup(x => x.GetByIdAndComercioIdAsync(1, 1)).ReturnsAsync(Builders.NovoCliente(1, 1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _termoRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<TermoAberturaConta>());
            _vendaRepository.Setup(x => x.GetSaldoDevedorPorAutorizadoAsync(1)).ReturnsAsync(new Dictionary<int, decimal>());
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoCliente(1, 1));
            _assinaturaRepository.Setup(x => x.GetGuidsPorTermosAberturaAsync(It.IsAny<List<int>>())).ReturnsAsync(new Dictionary<int, Guid>());
            _pessoaRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<PessoaAutorizada>());
            _renderer.Setup(x => x.RenderizarHtmlAsync(It.IsAny<TipoDocumento>(), It.IsAny<Dictionary<string, string>>())).ReturnsAsync("<html></html>");
            _renderer.Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>())).ReturnsAsync(Convert.ToBase64String("%PDF-1.7 termo"u8.ToArray()));
        }

        private ContaFiadoService CriarService() => new(
            _clienteRepository.Object, _comercioRepository.Object, _pessoaRepository.Object,
            _termoRepository.Object, _assinaturaRepository.Object, _vendaRepository.Object, _renderer.Object, _unitOfWork.Object);

        [Fact]
        public async Task GerarTermoAberturaAsync_ComTermoPendenteAnterior_DeveCancelarOAnteriorECriarDocumento()
        {
            var antigo = new TermoAberturaConta { TermoAberturaID = 3, ClienteID = 1, Versao = 2, Status = StatusTermoAbertura.Aguardando };
            var assinado = new TermoAberturaConta { TermoAberturaID = 1, ClienteID = 1, Versao = 1, Status = StatusTermoAbertura.Ativo };
            _termoRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<TermoAberturaConta> { antigo, assinado });

            var resposta = await CriarService().GerarTermoAberturaAsync(1, 1);

            resposta.DocumentoGuid.Should().NotBeEmpty();
            antigo.Status.Should().Be(StatusTermoAbertura.Cancelado);
            assinado.Status.Should().Be(StatusTermoAbertura.Ativo);
            _termoRepository.Verify(x => x.AddAsync(It.Is<TermoAberturaConta>(t => t.Versao == 3)), Times.Once);
            _assinaturaRepository.Verify(x => x.CancelarPorTermoAberturaIdAsync(3, It.IsAny<DateTime>()), Times.Once);
            _assinaturaRepository.Verify(x => x.AddAsync(It.Is<AssinaturaEletronica>(a =>
                a.TipoDocumento == TipoDocumento.TermoAberturaConta && a.TermoAbertura != null && a.VendaID == null
                && a.HashDocumentoOriginal != null)), Times.Once);
            _unitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task GerarTermoAberturaAsync_DeveListarRevogacaoPedidaForaDaTabela()
        {
            _pessoaRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<PessoaAutorizada>
            {
                new() { Nome = "Maria Ativa", Parentesco = ParentescoAutorizado.Conjuge, AutorizadaEm = DateTime.UtcNow },
                new() { Nome = "Jose Revogado", Parentesco = ParentescoAutorizado.Filho, AutorizadaEm = DateTime.UtcNow, RevogadaEm = DateTime.UtcNow },
                new() { Nome = "Ana Saindo", Parentesco = ParentescoAutorizado.Filho, AutorizadaEm = DateTime.UtcNow, RevogacaoSolicitadaEm = DateTime.UtcNow }
            });
            Dictionary<string, string>? tokens = null;
            _renderer.Setup(x => x.RenderizarHtmlAsync(TipoDocumento.TermoAberturaConta, It.IsAny<Dictionary<string, string>>()))
                .Callback<TipoDocumento, Dictionary<string, string>>((_, t) => tokens = t).ReturnsAsync("<html></html>");

            await CriarService().GerarTermoAberturaAsync(1, 1);

            tokens!["Html.TabelaAutorizados"].Should().Contain("Maria Ativa").And.NotContain("Jose Revogado").And.NotContain("Ana Saindo");
            tokens["Html.Revogacoes"].Should().Contain("Ana Saindo").And.NotContain("Jose Revogado");
            tokens["Termo.Versao"].Should().Be("1");
        }

        [Fact]
        public async Task EfetivarTermoAssinadoAsync_DeveAtivarSubstituirAnteriorEAutorizarQuemEstavaNoTermo()
        {
            var criadoEm = new DateTime(2026, 9, 10, 12, 0, 0);
            var termo = new TermoAberturaConta { TermoAberturaID = 5, ClienteID = 1, Status = StatusTermoAbertura.Aguardando, CriadoEm = criadoEm };
            var anterior = new TermoAberturaConta { TermoAberturaID = 2, ClienteID = 1, Status = StatusTermoAbertura.Ativo };
            var listada = new PessoaAutorizada { Nome = "Maria", CriadoEm = criadoEm.AddMinutes(-5) };
            var incluidaDepois = new PessoaAutorizada { Nome = "Joao", CriadoEm = criadoEm.AddMinutes(5) };
            var revogacaoNoTermo = new PessoaAutorizada { Nome = "Ana", CriadoEm = criadoEm.AddDays(-9), AutorizadaEm = criadoEm.AddDays(-8), RevogacaoSolicitadaEm = criadoEm.AddMinutes(-1) };
            var revogacaoDepois = new PessoaAutorizada { Nome = "Bia", CriadoEm = criadoEm.AddDays(-9), AutorizadaEm = criadoEm.AddDays(-8), RevogacaoSolicitadaEm = criadoEm.AddMinutes(1) };
            _termoRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(termo);
            _termoRepository.Setup(x => x.GetAtivoAsync(1)).ReturnsAsync(anterior);
            _pessoaRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<PessoaAutorizada> { listada, incluidaDepois, revogacaoNoTermo, revogacaoDepois });
            var data = new DateTime(2026, 9, 10, 13, 0, 0);

            await CriarService().EfetivarTermoAssinadoAsync(new AssinaturaEletronica { TermoAberturaID = 5 }, data);

            termo.Status.Should().Be(StatusTermoAbertura.Ativo);
            termo.AssinadoEm.Should().Be(data);
            anterior.Status.Should().Be(StatusTermoAbertura.Substituido);
            listada.AutorizadaEm.Should().Be(data);
            incluidaDepois.AutorizadaEm.Should().BeNull();
            revogacaoNoTermo.RevogadaEm.Should().Be(data);
            revogacaoDepois.RevogadaEm.Should().BeNull();
            revogacaoDepois.PodeComprar.Should().BeTrue();
        }

        [Fact]
        public async Task EfetivarTermoAssinadoAsync_TermoCancelado_DeveLancarBadRequest()
        {
            _termoRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new TermoAberturaConta { TermoAberturaID = 5, Status = StatusTermoAbertura.Cancelado });

            var act = async () => await CriarService().EfetivarTermoAssinadoAsync(new AssinaturaEletronica { TermoAberturaID = 5 }, DateTime.UtcNow);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task RevogarAutorizadoAsync_SoValeDepoisDeAssinarNovoTermo()
        {
            var pessoa = new PessoaAutorizada { PessoaAutorizadaID = 9, ClienteID = 1, Nome = "Joao", AutorizadaEm = DateTime.UtcNow };
            _pessoaRepository.Setup(x => x.GetDoClienteAsync(9, 1)).ReturnsAsync(pessoa);

            await CriarService().RevogarAutorizadoAsync(1, 9, 1);

            pessoa.RevogacaoSolicitadaEm.Should().NotBeNull();
            pessoa.RevogadaEm.Should().BeNull();
            pessoa.PodeComprar.Should().BeTrue();
        }

        [Fact]
        public async Task ObterAsync_ComPessoaPendente_DeveIndicarQuePrecisaNovoTermo()
        {
            _termoRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<TermoAberturaConta>
            {
                new() { TermoAberturaID = 2, Versao = 2, Status = StatusTermoAbertura.Ativo, AssinadoEm = DateTime.UtcNow },
                new() { TermoAberturaID = 1, Versao = 1, Status = StatusTermoAbertura.Substituido, AssinadoEm = DateTime.UtcNow.AddDays(-1) }
            });
            _pessoaRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<PessoaAutorizada>
            {
                new() { Nome = "Maria", AutorizadaEm = DateTime.UtcNow },
                new() { Nome = "Joao" }
            });

            var conta = await CriarService().ObterAsync(1, 1);

            conta.TermoAtivo.Should().BeTrue();
            conta.PrecisaNovoTermo.Should().BeTrue();
            conta.Autorizados.Select(a => a.Situacao).Should().Equal("Autorizada", "Pendente");
            conta.Termos.Select(t => t.Versao).Should().Equal(2, 1);
        }

        [Fact]
        public async Task ObterAsync_ClienteDeOutroComercio_DeveLancarNotFound()
        {
            var act = async () => await CriarService().ObterAsync(1, 999);

            await act.Should().ThrowAsync<NotFoundException>();
        }
    
        [Fact]
        public async Task GerarTermoAberturaNaTransacaoAsync_ComNovoLimite_DeveGravarNoTermoSemMudarOCliente()
        {
            var cliente = Builders.NovoCliente(1, 1, limiteCredito: 500m);
            Dictionary<string, string>? tokens = null;
            _renderer.Setup(x => x.RenderizarHtmlAsync(TipoDocumento.TermoAberturaConta, It.IsAny<Dictionary<string, string>>()))
                .Callback<TipoDocumento, Dictionary<string, string>>((_, t) => tokens = t).ReturnsAsync("<html></html>");

            await CriarService().GerarTermoAberturaNaTransacaoAsync(cliente, Builders.NovoComercio(1), novoLimite: 800m);

            cliente.LimiteCredito.Should().Be(500m);
            tokens!["Cliente.LimiteCredito"].Should().Be("R$ 800,00");
            _termoRepository.Verify(x => x.AddAsync(It.Is<TermoAberturaConta>(t => t.LimiteCredito == 800m)), Times.Once);
        }

        [Fact]
        public async Task GerarTermoAberturaNaTransacaoAsync_SemNovoLimite_DeveManterOLimiteDaVersaoPendente()
        {
            var pendente = new TermoAberturaConta { TermoAberturaID = 4, ClienteID = 1, Versao = 2, LimiteCredito = 800m, Status = StatusTermoAbertura.Aguardando };
            _termoRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<TermoAberturaConta> { pendente });

            await CriarService().GerarTermoAberturaNaTransacaoAsync(Builders.NovoCliente(1, 1, limiteCredito: 500m), Builders.NovoComercio(1));

            _termoRepository.Verify(x => x.AddAsync(It.Is<TermoAberturaConta>(t => t.LimiteCredito == 800m && t.Versao == 3)), Times.Once);
        }

        [Fact]
        public async Task EfetivarTermoAssinadoAsync_DeveAplicarAoClienteOLimiteDaVersaoAssinada()
        {
            var cliente = Builders.NovoCliente(1, 1, limiteCredito: 500m);
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);
            _termoRepository.Setup(x => x.GetByIdAsync(5)).ReturnsAsync(new TermoAberturaConta
            {
                TermoAberturaID = 5, ClienteID = 1, LimiteCredito = 800m, Status = StatusTermoAbertura.Aguardando
            });

            await CriarService().EfetivarTermoAssinadoAsync(new AssinaturaEletronica { TermoAberturaID = 5 }, DateTime.UtcNow);

            cliente.LimiteCredito.Should().Be(800m);
            _clienteRepository.Verify(x => x.UpdateAsync(cliente), Times.Once);
        }

        [Fact]
        public async Task ObterAsync_DeveInformarSaldoDeCadaAutorizadoELimitePendente()
        {
            _termoRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<TermoAberturaConta>
            {
                new() { TermoAberturaID = 3, Versao = 2, LimiteCredito = 800m, Status = StatusTermoAbertura.Aguardando },
                new() { TermoAberturaID = 2, Versao = 1, LimiteCredito = 500m, Status = StatusTermoAbertura.Ativo, AssinadoEm = DateTime.UtcNow }
            });
            _pessoaRepository.Setup(x => x.GetPorClienteAsync(1)).ReturnsAsync(new List<PessoaAutorizada>
            {
                new() { PessoaAutorizadaID = 7, Nome = "Com limite", LimiteCredito = 100m, AutorizadaEm = DateTime.UtcNow },
                new() { PessoaAutorizadaID = 8, Nome = "Sem limite", AutorizadaEm = DateTime.UtcNow }
            });
            _vendaRepository.Setup(x => x.GetSaldoDevedorPorAutorizadoAsync(1))
                .ReturnsAsync(new Dictionary<int, decimal> { [7] = 30m, [8] = 12m });

            var conta = await CriarService().ObterAsync(1, 1);

            conta.LimiteCredito.Should().Be(500m);
            conta.LimitePendente.Should().Be(800m);
            conta.Autorizados[0].SaldoDevedor.Should().Be(30m);
            conta.Autorizados[0].SaldoDisponivel.Should().Be(70m);
            conta.Autorizados[1].SaldoDevedor.Should().Be(12m);
            conta.Autorizados[1].SaldoDisponivel.Should().BeNull();
        }
    }
}
