using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class AssinaturaPlanoService : IAssinaturaPlanoService
    {
        private readonly IAssinaturaPlanoRepository _documentosVidaPlanoRepository;
        public AssinaturaPlanoService(IAssinaturaPlanoRepository AssinaturaPlanoRepository)
        {
            _documentosVidaPlanoRepository = AssinaturaPlanoRepository;
        }
    
        public async Task<PaginatedResponse<AssinaturaPlanoResponse>> GetAll(PaginationRequest request)
        {
            var (entidades, total) = await _documentosVidaPlanoRepository.GetPaginatedAsync(request.PageNumber, request.PageSize);
            var listaResponse = entidades.Adapt<List<AssinaturaPlanoResponse>>();

            return new PaginatedResponse<AssinaturaPlanoResponse>(
                listaResponse,
                request.PageNumber,
                request.PageSize,
                total
            );
        }
    
        public async Task<AssinaturaPlanoResponse> GetByIdAsync(int id)
        {
            var AssinaturaPlano = await _documentosVidaPlanoRepository.GetByIdAsync(id);
            if (AssinaturaPlano == null)
                throw new NotFoundException("AssinaturaPlano não encontrado.");
    
            return AssinaturaPlano.Adapt<AssinaturaPlanoResponse>();
        }
    }
}
