using FluentAssertions;
using Moq;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Communication.Venda;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;
using ninx.Tests.Helpers;
using Xunit;

namespace ninx.Tests.Services
{
    /// <summary>
    /// Testes unitários do VendaService: criação de venda, estorno, pagamento fiado
    /// (individual e geral) — cobrindo caminhos felizes e regras de negócio de falha.
    /// </summary>
    public class VendaServiceTests
    {
        private readonly Mock<IVendaRepository> _vendaRepository = new();
        private readonly Mock<IProdutoRepository> _produtoRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IEstoqueRepository> _estoqueRepository = new();
        private readonly Mock<IMovimentacaoEstoqueRepository> _movimentacaoEstoqueRepository = new();
        private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
        private readonly Mock<IComercioRepository> _comercioRepository = new();
        private readonly Mock<IUsuarioComercioRepository> _usuarioComercioRepository = new();
        private readonly Mock<IClienteRepository> _clienteRepository = new();
        private readonly Mock<IPagamentoVendaRepository> _pagamentoVendaRepository = new();
        private readonly Mock<IAssinaturaEletronicaRepository> _assinaturaEletronicaRepository = new();
        private readonly Mock<IDocumentoRendererService> _documentoRendererService = new();

        private VendaService CriarService() => new(
            _vendaRepository.Object,
            _produtoRepository.Object,
            _unitOfWork.Object,
            _estoqueRepository.Object,
            _movimentacaoEstoqueRepository.Object,
            _usuarioRepository.Object,
            _comercioRepository.Object,
            _usuarioComercioRepository.Object,
            _clienteRepository.Object,
            _pagamentoVendaRepository.Object,
            _assinaturaEletronicaRepository.Object,
            _documentoRendererService.Object);

        private void PrepararRenderizacaoDocumento()
        {
            _documentoRendererService
                .Setup(x => x.RenderizarHtmlAsync(It.IsAny<TipoDocumento>(), It.IsAny<Dictionary<string, string>>()))
                .ReturnsAsync("<html></html>");
            _documentoRendererService
                .Setup(x => x.ConverterParaPdfBase64Async(It.IsAny<string>()))
                .ReturnsAsync("base64pdf");
        }

        private CriarVendaRequest RequestVendaNormalValida() => new()
        {
            ComercioID = 1,
            UsuarioID = 1,
            ClienteID = 0,
            TipoVenda = (int)TipoVenda.Normal,
            ItensVenda = new List<ItemVendaRequest>
            {
                new() { ProdutoID = 1, Quantidade = 2 }
            },
            Pagamentos = new List<PagamentoVendaRequest>
            {
                new() { FormaPagamento = (int)FormaPagamento.Dinheiro, Valor = 20m }
            }
        };

        // ---------- CriarAsync ----------

        [Fact]
        public async Task CriarAsync_VendaNormalValida_DeveCriarComSucesso()
        {
            var request = RequestVendaNormalValida();
            var usuario = Builders.NovoUsuario(1);
            var comercio = Builders.NovoComercio(1);
            var produto = Builders.NovoProduto(1, 1, precoVenda: 10m);
            var estoque = Builders.NovoEstoque(1, 1, quantidade: 50m);

            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(usuario);
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(comercio);
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(true);
            _produtoRepository.Setup(x => x.GetProdutosById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Produto> { produto });
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(new List<Estoque> { estoque });

            var service = CriarService();
            var response = await service.CriarAsync(request);

            response.Should().NotBeNull();
            response.ComercioID.Should().Be(1);
            response.Total.Should().Be(20m);

            _vendaRepository.Verify(x => x.AddAsync(It.IsAny<Venda>()), Times.Once);
            _unitOfWork.Verify(x => x.CommitAsync(), Times.Once);
            _unitOfWork.Verify(x => x.RollbackAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarAsync_SemItens_DeveLancarBadRequest()
        {
            var request = RequestVendaNormalValida();
            request.ItensVenda = new List<ItemVendaRequest>();

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_TipoVendaInvalido_DeveLancarBadRequest()
        {
            var request = RequestVendaNormalValida();
            request.TipoVenda = 99;

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_UsuarioInativo_DeveLancarBadRequest()
        {
            var request = RequestVendaNormalValida();
            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1, ativo: false));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<BadRequestException>();
            _unitOfWork.Verify(x => x.BeginTransactionAsync(), Times.Never);
        }

        [Fact]
        public async Task CriarAsync_UsuarioSemVinculoNoComercio_DeveLancarBadRequest()
        {
            var request = RequestVendaNormalValida();
            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(false);

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_EstoqueInsuficiente_DeveLancarBadRequestEFazerRollback()
        {
            var request = RequestVendaNormalValida();
            request.ItensVenda[0].Quantidade = 1000;

            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(true);
            _produtoRepository.Setup(x => x.GetProdutosById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Produto> { Builders.NovoProduto(1, 1) });
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(new List<Estoque> { Builders.NovoEstoque(1, 1, quantidade: 5) });

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<BadRequestException>();
            _unitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task CriarAsync_VendaNormalComPagamentoInsuficiente_DeveLancarBadRequest()
        {
            var request = RequestVendaNormalValida();
            request.Pagamentos[0].Valor = 5m;

            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(true);
            _produtoRepository.Setup(x => x.GetProdutosById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Produto> { Builders.NovoProduto(1, 1, precoVenda: 10m) });
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(new List<Estoque> { Builders.NovoEstoque(1, 1, quantidade: 50) });

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_ProdutoNaoEncontrado_DeveLancarNotFound()
        {
            var request = RequestVendaNormalValida();

            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(true);
            _produtoRepository.Setup(x => x.GetProdutosById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Produto>());
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(new List<Estoque>());

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CriarAsync_VendaFiadoSemCliente_DeveLancarBadRequest()
        {
            var request = RequestVendaNormalValida();
            request.TipoVenda = (int)TipoVenda.Fiado;
            request.ClienteID = null;
            request.Pagamentos = new List<PagamentoVendaRequest>();

            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(true);
            _produtoRepository.Setup(x => x.GetProdutosById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Produto> { Builders.NovoProduto(1, 1) });
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(new List<Estoque> { Builders.NovoEstoque(1, 1, quantidade: 50) });

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_VendaFiadoExcedeLimiteCredito_DeveLancarBadRequest()
        {
            var request = RequestVendaNormalValida();
            request.TipoVenda = (int)TipoVenda.Fiado;
            request.ClienteID = 1;
            request.Pagamentos = new List<PagamentoVendaRequest>();
            request.ItensVenda[0].Quantidade = 100; // 100 * 10 = 1000, acima do limite de crédito de 500

            var cliente = Builders.NovoCliente(1, 1, limiteCredito: 500m);

            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(true);
            _produtoRepository.Setup(x => x.GetProdutosById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Produto> { Builders.NovoProduto(1, 1, precoVenda: 10m) });
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(new List<Estoque> { Builders.NovoEstoque(1, 1, quantidade: 500) });
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);
            _vendaRepository.Setup(x => x.GetVendasFiadoByClienteIDAsync(1)).ReturnsAsync(new List<Venda>());

            var service = CriarService();

            var act = async () => await service.CriarAsync(request);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_VendaFiadoValida_DeveGerarDocumentoDeAssinaturaPendente()
        {
            var request = RequestVendaNormalValida();
            request.TipoVenda = (int)TipoVenda.Fiado;
            request.ClienteID = 1;
            request.Pagamentos = new List<PagamentoVendaRequest>();

            var cliente = Builders.NovoCliente(1, 1, limiteCredito: 500m);

            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(true);
            _produtoRepository.Setup(x => x.GetProdutosById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Produto> { Builders.NovoProduto(1, 1, precoVenda: 10m) });
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(new List<Estoque> { Builders.NovoEstoque(1, 1, quantidade: 500) });
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);
            _vendaRepository.Setup(x => x.GetVendasFiadoByClienteIDAsync(1)).ReturnsAsync(new List<Venda>());
            PrepararRenderizacaoDocumento();

            var service = CriarService();
            var response = await service.CriarAsync(request);

            response.Documentos.Should().ContainSingle(d => !d.Assinado);
            _assinaturaEletronicaRepository.Verify(x => x.AddAsync(It.IsAny<AssinaturaEletronica>()), Times.Once);
        }

        [Fact]
        public async Task CriarAsync_VendaFiadoComEntradaZero_DeveCriarComSucesso()
        {
            var request = RequestVendaNormalValida();
            request.TipoVenda = (int)TipoVenda.Fiado;
            request.ClienteID = 1;
            request.Pagamentos = new List<PagamentoVendaRequest>
            {
                new() { FormaPagamento = (int)FormaPagamento.Dinheiro, Valor = 0m }
            };

            var cliente = Builders.NovoCliente(1, 1, limiteCredito: 500m);

            _usuarioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoUsuario(1));
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoComercio(1));
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, 1)).ReturnsAsync(true);
            _produtoRepository.Setup(x => x.GetProdutosById(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<Produto> { Builders.NovoProduto(1, 1, precoVenda: 10m) });
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(new List<Estoque> { Builders.NovoEstoque(1, 1, quantidade: 500) });
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);
            _vendaRepository.Setup(x => x.GetVendasFiadoByClienteIDAsync(1)).ReturnsAsync(new List<Venda>());
            PrepararRenderizacaoDocumento();

            var service = CriarService();
            var response = await service.CriarAsync(request);

            response.Total.Should().Be(20m);
        }

        // ---------- EstornarAsync ----------

        [Fact]
        public async Task EstornarAsync_VendaNaoEncontrada_DeveLancarNotFound()
        {
            _vendaRepository.Setup(x => x.GetByIdParaEstornoAsync(1)).ReturnsAsync((Venda?)null);

            var service = CriarService();

            var act = async () => await service.EstornarAsync(1, 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Theory]
        [InlineData(StatusVenda.Cancelada)]
        [InlineData(StatusVenda.Estornada)]
        public async Task EstornarAsync_VendaJaEstornadaOuCancelada_DeveLancarBadRequest(StatusVenda status)
        {
            var venda = Builders.NovaVenda(status: status);
            _vendaRepository.Setup(x => x.GetByIdParaEstornoAsync(1)).ReturnsAsync(venda);

            var service = CriarService();

            var act = async () => await service.EstornarAsync(1, 1);

            await act.Should().ThrowAsync<BadRequestException>();
            _unitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        }

        [Fact]
        public async Task EstornarAsync_VendaValida_DeveEstornarEDevolverEstoque()
        {
            var venda = Builders.NovaVenda(status: StatusVenda.Finalizada);
            venda.ItensVenda = new List<ItemVenda>
            {
                new() { ProdutoID = 1, ProdutoNome = "Produto Teste", Quantidade = 2 }
            };
            venda.PagamentosVenda = new List<PagamentoVenda>
            {
                new() { PagamentoID = 1, VendaID = venda.VendaID, FormaPagamento = FormaPagamento.Dinheiro, Valor = 20m, Status = StatusPagamento.Pago }
            };

            _vendaRepository.Setup(x => x.GetByIdParaEstornoAsync(1)).ReturnsAsync(venda);
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, venda.ComercioID)).ReturnsAsync(true);
            _estoqueRepository.Setup(x => x.GetByProdutosIdsAsync(It.IsAny<IEnumerable<int>>(), venda.ComercioID))
                .ReturnsAsync(new List<Estoque> { Builders.NovoEstoque(1, venda.ComercioID, quantidade: 10) });

            var service = CriarService();
            await service.EstornarAsync(1, 1);

            venda.Status.Should().Be(StatusVenda.Estornada);
            _estoqueRepository.Verify(x => x.UpdateBatchAsync(It.Is<IEnumerable<Estoque>>(e => e.Any(x => x.Quantidade == 12))), Times.Once);
            _pagamentoVendaRepository.Verify(x => x.AddBatchAsync(It.IsAny<IEnumerable<PagamentoVenda>>()), Times.Once);
            _unitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task EstornarAsync_UsuarioSemPermissao_DeveLancarBadRequestEFazerRollback()
        {
            var venda = Builders.NovaVenda(status: StatusVenda.Finalizada);
            _vendaRepository.Setup(x => x.GetByIdParaEstornoAsync(1)).ReturnsAsync(venda);
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, venda.ComercioID)).ReturnsAsync(false);

            var service = CriarService();

            var act = async () => await service.EstornarAsync(1, 1);

            await act.Should().ThrowAsync<BadRequestException>();
            _unitOfWork.Verify(x => x.RollbackAsync(), Times.Once);
        }

        // ---------- ReceberPagamentoFiadoAsync ----------

        [Fact]
        public async Task ReceberPagamentoFiadoAsync_ValorZeroOuNegativo_DeveLancarBadRequest()
        {
            var service = CriarService();

            var act = async () => await service.ReceberPagamentoFiadoAsync(1, 1, 0m, (int)FormaPagamento.Dinheiro);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ReceberPagamentoFiadoAsync_VendaNaoEncontrada_DeveLancarNotFound()
        {
            _vendaRepository.Setup(x => x.GetByIdParaPagamentoFiadoAsync(1)).ReturnsAsync((Venda?)null);
            var service = CriarService();

            var act = async () => await service.ReceberPagamentoFiadoAsync(1, 1, 10m, (int)FormaPagamento.Dinheiro);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ReceberPagamentoFiadoAsync_VendaNaoEhFiado_DeveLancarBadRequest()
        {
            var venda = Builders.NovaVenda(tipoVenda: TipoVenda.Normal, status: StatusVenda.Aberta);
            _vendaRepository.Setup(x => x.GetByIdParaPagamentoFiadoAsync(1)).ReturnsAsync(venda);
            var service = CriarService();

            var act = async () => await service.ReceberPagamentoFiadoAsync(1, 1, 10m, (int)FormaPagamento.Dinheiro);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ReceberPagamentoFiadoAsync_ComAssinaturaPendente_DeveLancarBadRequest()
        {
            var venda = Builders.NovaVenda(tipoVenda: TipoVenda.Fiado, status: StatusVenda.Aberta, clienteId: 1, total: 100m);
            _vendaRepository.Setup(x => x.GetByIdParaPagamentoFiadoAsync(1)).ReturnsAsync(venda);
            _assinaturaEletronicaRepository.Setup(x => x.ExisteAssinaturaPendenteAsync(1)).ReturnsAsync(true);

            var service = CriarService();

            var act = async () => await service.ReceberPagamentoFiadoAsync(1, 1, 10m, (int)FormaPagamento.Dinheiro);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ReceberPagamentoFiadoAsync_ValorMaiorQueSaldoDevedor_DeveLancarBadRequest()
        {
            var venda = Builders.NovaVenda(tipoVenda: TipoVenda.Fiado, status: StatusVenda.Aberta, clienteId: 1, total: 100m);
            _vendaRepository.Setup(x => x.GetByIdParaPagamentoFiadoAsync(1)).ReturnsAsync(venda);
            _assinaturaEletronicaRepository.Setup(x => x.ExisteAssinaturaPendenteAsync(1)).ReturnsAsync(false);
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, venda.ComercioID)).ReturnsAsync(true);

            var service = CriarService();

            var act = async () => await service.ReceberPagamentoFiadoAsync(1, 1, 150m, (int)FormaPagamento.Dinheiro);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ReceberPagamentoFiadoAsync_PagamentoValido_DeveRegistrarERetornarGuid()
        {
            var venda = Builders.NovaVenda(tipoVenda: TipoVenda.Fiado, status: StatusVenda.Aberta, clienteId: 1, total: 100m);
            _vendaRepository.Setup(x => x.GetByIdParaPagamentoFiadoAsync(1)).ReturnsAsync(venda);
            _assinaturaEletronicaRepository.Setup(x => x.ExisteAssinaturaPendenteAsync(1)).ReturnsAsync(false);
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, venda.ComercioID)).ReturnsAsync(true);
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoCliente(1, venda.ComercioID));
            _comercioRepository.Setup(x => x.GetByIdAsync(venda.ComercioID)).ReturnsAsync(Builders.NovoComercio(venda.ComercioID));
            PrepararRenderizacaoDocumento();

            var service = CriarService();
            var guid = await service.ReceberPagamentoFiadoAsync(1, 1, 50m, (int)FormaPagamento.Pix);

            guid.Should().NotBe(Guid.Empty);
            _pagamentoVendaRepository.Verify(x => x.AddAsync(It.Is<PagamentoVenda>(p => p.Valor == 50m)), Times.Once);
            _unitOfWork.Verify(x => x.CommitAsync(), Times.Once);
        }

        // ---------- ReceberPagamentoGeralFiadoAsync ----------

        [Fact]
        public async Task ReceberPagamentoGeralFiadoAsync_SemVendasFiado_DeveLancarNotFound()
        {
            _vendaRepository.Setup(x => x.GetVendasFiadoAtivasPorClienteAsync(1)).ReturnsAsync(new List<Venda>());
            var service = CriarService();

            var act = async () => await service.ReceberPagamentoGeralFiadoAsync(1, 1, 100m, (int)FormaPagamento.Dinheiro);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task ReceberPagamentoGeralFiadoAsync_ValorSuperiorADivida_DeveLancarBadRequest()
        {
            var venda = Builders.NovaVenda(tipoVenda: TipoVenda.Fiado, status: StatusVenda.Aberta, clienteId: 1, total: 100m);
            _vendaRepository.Setup(x => x.GetVendasFiadoAtivasPorClienteAsync(1)).ReturnsAsync(new List<Venda> { venda });
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, venda.ComercioID)).ReturnsAsync(true);

            var service = CriarService();

            var act = async () => await service.ReceberPagamentoGeralFiadoAsync(1, 1, 200m, (int)FormaPagamento.Dinheiro);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task ReceberPagamentoGeralFiadoAsync_AbatendoVariasVendas_DeveQuitarGlobalmente()
        {
            var venda1 = Builders.NovaVenda(id: 1, tipoVenda: TipoVenda.Fiado, status: StatusVenda.Aberta, clienteId: 1, total: 60m);
            var venda2 = Builders.NovaVenda(id: 2, tipoVenda: TipoVenda.Fiado, status: StatusVenda.Aberta, clienteId: 1, total: 40m);
            _vendaRepository.Setup(x => x.GetVendasFiadoAtivasPorClienteAsync(1)).ReturnsAsync(new List<Venda> { venda1, venda2 });
            _usuarioComercioRepository.Setup(x => x.ExisteVinculoAsync(1, venda1.ComercioID)).ReturnsAsync(true);
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(Builders.NovoCliente(1, venda1.ComercioID));
            _comercioRepository.Setup(x => x.GetByIdAsync(venda1.ComercioID)).ReturnsAsync(Builders.NovoComercio(venda1.ComercioID));
            PrepararRenderizacaoDocumento();

            var service = CriarService();
            var guid = await service.ReceberPagamentoGeralFiadoAsync(1, 1, 100m, (int)FormaPagamento.Dinheiro);

            guid.Should().NotBe(Guid.Empty);
            _pagamentoVendaRepository.Verify(x => x.AddAsync(It.IsAny<PagamentoVenda>()), Times.Exactly(2));
            _assinaturaEletronicaRepository.Verify(x => x.AddAsync(It.IsAny<AssinaturaEletronica>()), Times.Exactly(2));
        }

        // ---------- GetByVendaIdAsync ----------

        [Fact]
        public async Task GetByVendaIdAsync_VendaDeOutroComercio_DeveLancarNotFound()
        {
            var venda = Builders.NovaVenda(id: 1, comercioId: 5);
            _vendaRepository.Setup(x => x.GetByIdParaDetalheAsync(1)).ReturnsAsync(venda);

            var service = CriarService();

            var act = async () => await service.GetByVendaIdAsync(1, comercioId: 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetByVendaIdAsync_VendaValida_DeveRetornarComSaldoDevedorCalculado()
        {
            var venda = Builders.NovaVenda(id: 1, comercioId: 1, total: 100m);
            venda.PagamentosVenda = new List<PagamentoVenda>
            {
                new() { Status = StatusPagamento.Pago, Valor = 30m }
            };
            _vendaRepository.Setup(x => x.GetByIdParaDetalheAsync(1)).ReturnsAsync(venda);
            _assinaturaEletronicaRepository.Setup(x => x.GetDocumentosPorVendaIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<VendaDocumentoResumo>());

            var service = CriarService();
            var response = await service.GetByVendaIdAsync(1, 1);

            response.ValorPago.Should().Be(30m);
            response.SaldoDevedor.Should().Be(70m);
        }
    }
}
