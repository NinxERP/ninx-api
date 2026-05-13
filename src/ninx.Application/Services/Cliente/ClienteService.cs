using Mapster;
using ninx.Communication;
using ninx.Communication;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class ClienteService : IClienteService
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ClienteService(IClienteRepository clienteRepository, IUnitOfWork unitOfWork)
        {
            _clienteRepository = clienteRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<PaginatedResponse<ClienteResponse>> GetAllByComercioId(int comercioId, PaginationRequest request)
        {
            var (entidades, total) = await _clienteRepository.GetPaginatedAsync(request.PageNumber, request.PageSize);
            entidades = entidades.Where(c => c.ComercioID == comercioId).ToList();
            var listaResponse = entidades.Adapt<List<ClienteResponse>>();

            return new PaginatedResponse<ClienteResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
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
    }
}
    
