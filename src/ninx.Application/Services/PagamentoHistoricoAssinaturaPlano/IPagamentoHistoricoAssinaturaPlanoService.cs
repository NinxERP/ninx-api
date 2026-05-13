using ninx.Communication;
using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IPagamentoHistoricoAssinaturaPlanoService
    {
        Task RegistrarPagamentos(PagamentoHistoricoAssinaturaPlanoRequest request, Permissao permissao);
    }
}
