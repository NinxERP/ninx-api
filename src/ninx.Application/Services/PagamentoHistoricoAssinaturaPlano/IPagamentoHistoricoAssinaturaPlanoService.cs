using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IPagamentoHistoricoAssinaturaPlanoService
    {
        Task RegistrarPagamentos(PagamentoHistoricoAssinaturaPlanoRequest request, int usuarioLogadoId);
        Task<PaginatedResponse<PagamentoHistoricoAssinaturaPlanoResponse>> GetHistoricoByComercioIdAsync(int comercioId, int pesoLogado, PaginationRequest request);
    }
}
