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
    public class EstoqueServiceTests
    {
        private readonly Mock<IEstoqueRepository> _estoqueRepository = new();
        private readonly Mock<IComercioRepository> _comercioRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();

        private EstoqueService CriarService() => new(_estoqueRepository.Object, _comercioRepository.Object, _unitOfWork.Object);

        [Fact]
        public async Task GetByIdAsync_EstoqueDeOutroComercio_DeveLancarNotFound()
        {
            var estoque = Builders.NovoEstoque(1, comercioId: 2);
            _estoqueRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(estoque);
            var service = CriarService();

            var act = async () => await service.GetByIdAsync(1, comercioId: 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CreateAsync_ComercioInexistente_DeveLancarNotFound()
        {
            _comercioRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Comercio?)null);
            var service = CriarService();

            var act = async () => await service.CreateAsync(new EstoqueRequest { ProdutoID = 1, Quantidade = 10 }, comercioId: 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task UpdateAsync_ComercioDiferente_DeveLancarForbidden()
        {
            var estoque = Builders.NovoEstoque(1, comercioId: 2);
            _estoqueRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(estoque);
            var service = CriarService();

            var act = async () => await service.UpdateAsync(1, new EstoqueRequest { ProdutoID = 1, Quantidade = 10 }, comercioId: 1);

            await act.Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task UpdateAsync_QuantidadeNegativa_DeveLancarBadRequest()
        {
            var estoque = Builders.NovoEstoque(1, comercioId: 1);
            _estoqueRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(estoque);
            var service = CriarService();

            var act = async () => await service.UpdateAsync(1, new EstoqueRequest { ProdutoID = 1, Quantidade = -5 }, comercioId: 1);

            await act.Should().ThrowAsync<BadRequestException>();
        }

        [Fact]
        public async Task DeleteAsync_EstoqueInexistente_DeveLancarNotFound()
        {
            _estoqueRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Estoque?)null);
            var service = CriarService();

            var act = async () => await service.DeleteAsync(1, 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DeleteAsync_Valido_DeveDeletar()
        {
            var estoque = Builders.NovoEstoque(1, comercioId: 1);
            _estoqueRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(estoque);
            var service = CriarService();

            await service.DeleteAsync(1, 1);

            _estoqueRepository.Verify(x => x.DeleteAsync(estoque), Times.Once);
        }
    }
}
