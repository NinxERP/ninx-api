using Mapster;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;

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
        public async Task<IEnumerable<ClienteResponse>> GetAll()
        {
            var clientes = await _clienteRepository.GetAllAsync();
            return clientes.Adapt<IEnumerable<ClienteResponse>>();
        }

        public async Task<IEnumerable<ClienteResponse>> GetByClienteId(int clienteId)
        {
            var clientes = await _clienteRepository.GetByIdAsync(clienteId);
            if (clientes == null)
            throw new NotFoundException("Cliente não encontrado.");
            
            return clientes.Adapt<IEnumerable<ClienteResponse>>();
        }

        public async Task<ClienteResponse> CriarAsync(ClienteRequest request)
        {
            var cliente = request.Adapt<Cliente>();

            await _clienteRepository.AddAsync(cliente);
            await _unitOfWork.SaveChangesAsync();

            return cliente.Adapt<ClienteResponse>();
        }

        public async Task<ClienteResponse> AtualizarAsync(int id, int usuarioLogadoId, ClienteRequest request)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);

            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            request.Adapt(cliente);
            cliente.AtualizadoEm = DateTime.UtcNow;

            await _clienteRepository.UpdateAsync(cliente);
            await _unitOfWork.SaveChangesAsync();

            return cliente.Adapt<ClienteResponse>();
        }

        public async Task DesativarAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            cliente.Ativo = false;
            cliente.AtualizadoEm = DateTime.UtcNow;

            await _clienteRepository.UpdateAsync(cliente);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ClienteResponse> GetByIdAsync(int id)
        {
            var cliente = await _clienteRepository.GetByIdAsync(id);
            if (cliente == null)
                throw new NotFoundException("Cliente não encontrado.");

            return cliente.Adapt<ClienteResponse>();
        }
    }
}
    
