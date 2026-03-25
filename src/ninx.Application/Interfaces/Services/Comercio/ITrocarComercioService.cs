using ninx.Communication.Request;

namespace ninx.Application.Interfaces.Services
{
    public interface ITrocarComercioService
    {
        Task<string?> TrocarAsync (TrocarComercioRequest request);
    }
}
