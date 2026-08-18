using ninx.Communication;
using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IAssinaturaPlanoService
    {
        Task<AssinaturaPlanoResponse> GetByIdAsync(int id);
        Task<PaginatedResponse<AssinaturaPlanoResponse>> GetAll(PaginationRequest request);
        Task<AssinaturaPlanoResponse> GetByComercioIdAsync(int comercioId, Permissao permissaoLogado);
    }
}
