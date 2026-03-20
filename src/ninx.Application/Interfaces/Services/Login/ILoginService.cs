using ninx.Communication.Request;

namespace ninx.Application.Interfaces.Services.Login
{
    public interface ILoginService
    {
        public Task<string?> LoginAsync(LoginRequest request);
    }
}
