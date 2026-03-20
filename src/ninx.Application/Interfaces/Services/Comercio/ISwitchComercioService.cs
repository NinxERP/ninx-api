using ninx.Communication.Request;

namespace ninx.Application.Interfaces.Services.SwitchComercio
{
    public interface ISwitchComercioService
    {
        Task<string?> TrocarAsync (SwitchComercioRequest request);
    }
}
