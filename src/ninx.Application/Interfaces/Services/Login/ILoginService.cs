using ninx.Communication.Request;
using ninx.Communication.Response;

namespace ninx.Application.Interfaces.Services
{
    public interface ILoginService
    {
        public Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}
