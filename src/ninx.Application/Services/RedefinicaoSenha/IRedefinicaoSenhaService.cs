using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IRedefinicaoSenhaService
    {
        Task SolicitarAsync(SolicitarRedefinicaoSenhaRequest request);
        Task ConfirmarAsync(ConfirmarRedefinicaoSenhaRequest request);
    }
}
