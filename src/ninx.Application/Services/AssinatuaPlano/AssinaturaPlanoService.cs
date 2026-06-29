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
    
        public async Task<PaginatedResponse<AssinaturaPlanoResponse>> GetAll(PaginationRequest request)
        {
            var (entidades, total) = await _AssinaturaPlanoRepository.GetPaginatedAsync(request.PageNumber, request.PageSize);
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
            var AssinaturaPlano = await _AssinaturaPlanoRepository.GetByIdAsync(id);
            if (AssinaturaPlano == null)
                throw new NotFoundException("AssinaturaPlano não encontrado.");
    
            return AssinaturaPlano.Adapt<AssinaturaPlanoResponse>();
        }
    }
}
