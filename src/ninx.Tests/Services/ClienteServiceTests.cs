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
    public class ClienteServiceTests
    {
        private readonly Mock<IClienteRepository> _clienteRepository = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly Mock<IVendaRepository> _vendaRepository = new();

        private ClienteService CriarService() => new(_clienteRepository.Object, _unitOfWork.Object, _vendaRepository.Object);

        [Fact]
        public async Task GetByIdAsync_ClienteExistente_DeveRetornarResponse()
        {
            var cliente = Builders.NovoCliente(1, 1);
            _clienteRepository.Setup(x => x.GetByIdAndComercioIdAsync(1, 1)).ReturnsAsync(cliente);

            var service = CriarService();
            var response = await service.GetByIdAsync(1, 1);

            response.ClienteID.Should().Be(1);
            response.Nome.Should().Be(cliente.Nome);
        }

        [Fact]
        public async Task GetByIdAsync_ClienteInexistente_DeveLancarNotFound()
        {
            _clienteRepository.Setup(x => x.GetByIdAndComercioIdAsync(1, 1)).ReturnsAsync((Cliente?)null);

            var service = CriarService();

            var act = async () => await service.GetByIdAsync(1, 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task CriarAsync_DeveNormalizarCpfECepEUf()
        {
            var request = new ClienteRequest
            {
                Nome = "Fulano",
                Cpf = "123.456.789-09",
                EnderecoLogradouro = "Rua A",
                EnderecoNumero = "1",
                EnderecoBairro = "Bairro",
                EnderecoCidade = "Cidade",
                EnderecoUF = "sp",
                EnderecoCEP = "01000-000"
            };

            Cliente? clienteCriado = null;
            _clienteRepository.Setup(x => x.AddAsync(It.IsAny<Cliente>()))
                .Callback<Cliente>(c => clienteCriado = c)
                .ReturnsAsync((Cliente c) => c);

            var service = CriarService();
            await service.CriarAsync(request, comercioId: 1);

            clienteCriado.Should().NotBeNull();
            clienteCriado!.Cpf.Should().Be("12345678909");
            clienteCriado.EnderecoCEP.Should().Be("01000000");
            clienteCriado.EnderecoUF.Should().Be("SP");
            clienteCriado.ComercioID.Should().Be(1);
        }

        [Fact]
        public async Task AtualizarAsync_ClienteDeOutroComercio_DeveLancarNotFound()
        {
            var cliente = Builders.NovoCliente(1, comercioId: 2);
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);

            var request = new ClienteRequest
            {
                Nome = "Fulano",
                Cpf = "12345678909",
                EnderecoLogradouro = "Rua A",
                EnderecoNumero = "1",
                EnderecoBairro = "Bairro",
                EnderecoCidade = "Cidade",
                EnderecoUF = "SP",
                EnderecoCEP = "01000000"
            };

            var service = CriarService();

            var act = async () => await service.AtualizarAsync(1, usuarioLogadoId: 1, request, comercioId: 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DesativarAsync_ClienteInexistente_DeveLancarNotFound()
        {
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync((Cliente?)null);
            var service = CriarService();

            var act = async () => await service.DesativarAsync(1, 1);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact]
        public async Task DesativarAsync_ClienteValido_DeveDesativar()
        {
            var cliente = Builders.NovoCliente(1, 1);
            _clienteRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(cliente);

            var service = CriarService();
            await service.DesativarAsync(1, 1);

            cliente.Ativo.Should().BeFalse();
            _clienteRepository.Verify(x => x.UpdateAsync(cliente), Times.Once);
        }

        [Fact]
        public async Task GetAllByComercioId_DevePopularSaldoDevedorPorCliente()
        {
            var cliente = Builders.NovoCliente(1, 1);
            _clienteRepository.Setup(x => x.GetClienteComercioByComercioId(1, It.IsAny<PaginationRequest>()))
                .ReturnsAsync((new List<Cliente> { cliente }, 1, new MetricsSummary()));
            _vendaRepository.Setup(x => x.GetSaldoDevedorClientesPorComercio(1))
                .ReturnsAsync(new Dictionary<int, decimal> { { 1, 42.5m } });

            var service = CriarService();
            var response = await service.GetAllByComercioId(1, new PaginationRequest());

            response.Data.Should().ContainSingle(c => c.SaldoDevedor == 42.5m);
        }
    }
}
