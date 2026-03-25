using Mapster;
using ninx.Application.Interfaces.Services;
using ninx.Communication.Request;
using ninx.Communication.Response;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Application.Services
{
    public class ComercioService : IComercioService
    {
        public readonly IComercioRepository _comercioRepository;
        public ComercioService(IComercioRepository comercioRepository)
        {
            _comercioRepository = comercioRepository;
        }
        public async Task<IEnumerable<ComercioResponse>> GetAll()
        {
            var comercios = await _comercioRepository.GetAllAsync();
            return comercios.Adapt<IEnumerable<ComercioResponse>>();
        }

        public async Task<IEnumerable<ComercioResponse>> GetByUsuarioId(int usuarioId)
        {
            var comercios = _comercioRepository.GetByUsuarioId(usuarioId);
            return comercios.Adapt<IEnumerable<ComercioResponse>>();
        }

        public async Task<ComercioResponse> CriarAsync(ComercioRequest request)
        {
            var comercio = request.Adapt<Comercio>();

            await _comercioRepository.AddAsync(comercio);
            return comercio.Adapt<ComercioResponse>();
        }

        public async Task<ComercioResponse> AtualizarAsync(int id, ComercioRequest request)
        {
            var comercio = await _comercioRepository.GetByIdAsync(id);
            if (comercio == null)
            {
              throw new NotFoundException("comercio não encontrado.");
            }

            request.Adapt(comercio);
            comercio.AtualizadoEm = DateTime.Now;

            await _comercioRepository.UpdateAsync(comercio);
            return comercio.Adapt<ComercioResponse>();
        }
        public async Task DesativarAsync(int id)
        {
            var comercio = await _comercioRepository.GetByIdAsync(id);
            if (comercio == null)
            {
                throw new NotFoundException("Comercio não encontrado");
            }

            comercio.Ativo = false;
            comercio.AtualizadoEm = DateTime.Now;
            await _comercioRepository.UpdateAsync(comercio);
        }

        public async Task<ComercioResponse> GetByIdAsync(int id)
        {
            var comercio = await _comercioRepository.GetByIdAsync(id);
            if (comercio == null)
            {
                throw new NotFoundException("Comercio não encontrado");
            }
            return comercio.Adapt<ComercioResponse>();
        }
    }
}
