using ninx.Communication;

namespace ninx.Application.Services
{
    public interface ITrocarComercioService
    {
        Task<string> TrocarAsync(int comercioID, int usuarioLogadoId);
    }
}
