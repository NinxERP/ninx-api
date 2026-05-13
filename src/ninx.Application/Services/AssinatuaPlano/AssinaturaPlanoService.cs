using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class AssinaturaPlanoService : IAssinaturaPlanoService
    {
        private readonly IAssinaturaPlanoRepository _AssinaturaPlanoRepository;
        public AssinaturaPlanoService(IAssinaturaPlanoRepository AssinaturaPlanoRepository)
        {
            _AssinaturaPlanoRepository = AssinaturaPlanoRepository;
        }
    
        public async Task<PaginatedResponse<AssinaturaPlanoResponse>> GetAll(int pageNumber = 1, int pageSize = 10)
        {
            var assinaturas = await _AssinaturaPlanoRepository.GetAllAsync();
            var totalRecords = assinaturas.Count();

            var paginatedData = assinaturas
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var data = paginatedData.Adapt<List<AssinaturaPlanoResponse>>();

            return new PaginatedResponse<AssinaturaPlanoResponse>(data, pageNumber, pageSize, totalRecords);
        }
    
        public async Task<AssinaturaPlanoResponse> GetByIdAsync(int id)
        {
            var AssinaturaPlano = await _AssinaturaPlanoRepository.GetByIdAsync(id);
            if (AssinaturaPlano == null)
                throw new NotFoundException("AssinaturaPlano não encontrado.");
    
            return AssinaturaPlano.Adapt<AssinaturaPlanoResponse>();
        }
    }
}
