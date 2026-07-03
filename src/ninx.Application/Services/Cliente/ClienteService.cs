using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IVendaRepository _vendaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ClienteService(IClienteRepository clienteRepository, IUnitOfWork unitOfWork, IVendaRepository vendaRepository)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
            _vendaRepository = vendaRepository;
        }
        public async Task<PaginatedResponse<ClienteResponse>> GetAllByComercioId(int comercioId, PaginationRequest request)
        {
            var (entidades, total, totalAtivo) = await _clienteRepository.GetClienteComercioByComercioId(comercioId, request);
            var listaResponse = entidades.Adapt<List<ClienteResponse>>();
            var saldoDevedor = await _vendaRepository.GetSaldoDevedorClientesPorComercio(comercioId);
            listaResponse.ForEach(cliente =>
            {
                if (saldoDevedor.TryGetValue(cliente.ClienteID, out decimal saldo))
                {
                    cliente.SaldoDevedor = saldo;
                }
                else
                {
                    cliente.SaldoDevedor = 0;
                }
            });
            return new PaginatedResponse<ClienteResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total,
                totalAtivo,
                0,
                0,
                0,
                (total - totalAtivo)
            );
        }

        public async Task<IEnumerable<ClienteResponse>> GetByIdAsync(int clienteId, int comercioId)
        {
            var clientes = await _clienteRepository.GetByIdAsync(clienteId);
            if (clientes == null) throw new NotFoundException("Cliente não encontrado.");
            if (clientes.ComercioID != comercioId) throw new NotFoundException("Cliente não pertence ao seu comercio.");
            return clientes.Adapt<IEnumerable<ClienteResponse>>();
        }

        public async Task<ClienteResponse> CriarAsync(ClienteRequest request, int comercioId)
        {
            var cliente = request.Adapt<Cliente>();
            cliente.ComercioID = comercioId;

            await _clienteRepository.AddAsync(cliente);
            await _unitOfWork.SaveChangesAsync();

            return cliente.Adapt<ClienteResponse>();
        }

        public async Task<ClienteResponse> AtualizarAsync(int id, int usuarioLogadoId, ClienteRequest request, int comercioId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null) throw new NotFoundException("Cliente não encontrado.");
            if (cliente.ComercioID != comercioId) throw new NotFoundException("Cliente não pertence ao seu comercio.");

            request.Adapt(cliente);
            cliente.AtualizadoEm = DateTime.UtcNow;

            await _clienteRepository.UpdateAsync(cliente);
            await _unitOfWork.SaveChangesAsync();

            return cliente.Adapt<ClienteResponse>();
        }

        public async Task DesativarAsync(int id, int comercioId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null) throw new NotFoundException("Cliente não encontrado.");
            if (cliente.ComercioID != comercioId) throw new NotFoundException("Cliente não pertence ao seu comercio.");

            cliente.Ativo = false;
            cliente.AtualizadoEm = DateTime.UtcNow;

            await _clienteRepository.UpdateAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ClienteResponse>> GetByNomeAsync(string Nome, int comercioId)
        {
            var clientes = await _clienteRepository.GetByNomeAsync(Nome, comercioId);
            if (clientes == null) throw new NotFoundException("Cliente não encontrado.");
            if (!clientes.Any(x => x.ComercioID == comercioId)) throw new NotFoundException("Cliente não pertence ao seu comercio.");
            return clientes.Adapt<IEnumerable<ClienteResponse>>();
        }
    }
}
    
