using ninx.Communication.Request.Login;

namespace ninx.Application.Interfaces.Services.Login
{
    public interface ITrocarComercioService
    {
        Task<string?> TrocarAsync (TrocarComercioRequest request);
    }
}
