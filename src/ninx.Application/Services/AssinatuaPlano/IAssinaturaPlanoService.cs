using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IAssinaturaPlanoService
    {
        Task<AssinaturaPlanoResponse> GetByIdAsync(int id);
        Task<PaginatedResponse<AssinaturaPlanoResponse>> GetAll(PaginationRequest request);
    }
}
