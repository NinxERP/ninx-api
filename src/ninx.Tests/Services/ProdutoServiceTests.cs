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
    public class ProdutoServiceTests
    {
        private readonly Mock<IProdutoRepository> _produtoRepository = new();
        private readonly Mock<IEstoqueRepository> _estoqueRepository = new();
        private readonly Mock<ICategoriaProdutoRepository> _categoriaProdutoRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private ProdutoService CriarService() => new(
            _produtoRepository.Object,
            _estoqueRepository.Object,
            _categoriaProdutoRepository.Object,
            _unitOfWork.Object);

        [Fact]
        public async Task CriarAsync_ComercioDivergente_DeveLancarBadRequest()
        {
            var service = CriarService();
            var request = new CriarProdutoRequest { ComercioID = 2, Nome = "Produto", PrecoVenda = 10 };

            var act = async () => await service.CriarAsync(request, comercioID: 1);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_CategoriaInexistenteNoComercio_DeveLancarBadRequest()
        {
            _categoriaProdutoRepository.Setup(x => x.GetByIdAndComercioIdAsync(5, 1)).ReturnsAsync((CategoriaProduto?)null);
            var service = CriarService();
            var request = new CriarProdutoRequest { ComercioID = 1, CategoriaID = 5, Nome = "Produto", PrecoVenda = 10 };

            var act = async () => await service.CriarAsync(request, comercioID: 1);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task CriarAsync_ComEstoqueInicial_DeveCriarRegistroDeEstoque()
        {
            var service = CriarService();
            var request = new CriarProdutoRequest { ComercioID = 1, Nome = "Produto", PrecoVenda = 10, EstoqueInicial = 20 };

            await service.CriarAsync(request, comercioID: 1);

            _estoqueRepository.Verify(x => x.AddAsync(It.Is<Estoque>(e => e.Quantidade == 20)), Times.Once);
        }

        [Fact]
        public async Task CriarAsync_SemEstoqueInicialNemMinimo_NaoDeveCriarRegistroDeEstoque()
        {
            var service = CriarService();
            var request = new CriarProdutoRequest { ComercioID = 1, Nome = "Produto", PrecoVenda = 10 };

            await service.CriarAsync(request, comercioID: 1);

            _estoqueRepository.Verify(x => x.AddAsync(It.IsAny<Estoque>()), Times.Never);
        }

        [Fact]
        public async Task AtualizarAsync_ProdutoInexistente_DeveLancarNotFound()
        {
            _produtoRepository.Setup(x => x.GetByIdAndComercioAsync(1, 1)).ReturnsAsync((Produto?)null);
            var service = CriarService();

            var act = async () => await service.AtualizarAsync(1, 1, new AtualizarProdutoRequest { Nome = "X", PrecoVenda = 1 });

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task GetByIdAsync_ProdutoInexistente_DeveLancarNotFound()
        {
            _produtoRepository.Setup(x => x.GetByIdAndComercioAsync(1, 1)).ReturnsAsync((Produto?)null);
            var service = CriarService();

            var act = async () => await service.GetByIdAsync(1, 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DesativarAsync_ProdutoValido_DeveDesativar()
        {
            var produto = Builders.NovoProduto(1, 1);
            _produtoRepository.Setup(x => x.GetByIdAndComercioAsync(1, 1)).ReturnsAsync(produto);
            var service = CriarService();

            await service.DesativarAsync(1, 1);

            produto.Ativo.Should().BeFalse();
        }
    }
}
