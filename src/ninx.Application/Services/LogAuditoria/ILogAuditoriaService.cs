using ninx.Communication;

namespace ninx.Application.Services
{
    public interface ILogAuditoriaService
    {
        Task RegistrarAsync(int usuarioId, int? comercioId, string acao, string entidade, int entidadeId, string? detalhes = null);
        Task<PaginatedResponse<LogAuditoriaResponse>> GetAllAsync(int usuarioIdLogado, int? comercioId, int? usuarioId, PaginationRequest request);
    }
}
