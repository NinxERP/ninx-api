using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IPagamentoHistoricoAssinaturaPlanoService
    {
        Task RegistrarPagamentos(PagamentoHistoricoAssinaturaPlanoRequest request, int usuarioLogadoId);
        Task<PaginatedResponse<PagamentoHistoricoAssinaturaPlanoResponse>> GetHistoricoByComercioIdAsync(int comercioId, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado, PaginationRequest request);
    }
}
