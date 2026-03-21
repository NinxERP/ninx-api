using ninx.Communication.Request.Login;
using ninx.Communication.Response.Login;

namespace ninx.Application.Interfaces.Services.Login
{
    public interface ILoginService
    {
        public Task<LoginResponse?> LoginAsync(LoginRequest request);
    }
}
