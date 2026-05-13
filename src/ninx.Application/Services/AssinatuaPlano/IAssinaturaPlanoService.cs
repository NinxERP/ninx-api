using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IAssinaturaPlanoService
    {
        Task<AssinaturaPlanoResponse> GetByIdAsync(int id);
        Task<PaginatedResponse<AssinaturaPlanoResponse>> GetAll(int pageNumber = 1, int pageSize = 10);
    }
}
