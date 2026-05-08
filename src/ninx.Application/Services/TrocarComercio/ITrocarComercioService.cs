using ninx.Communication.Request;

namespace ninx.Application.Services
{
    public interface ITrocarComercioService
    {
        Task<string> TrocarAsync(int comercioID, int usuarioLogadoId);
    }
}
