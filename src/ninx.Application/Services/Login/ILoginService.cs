using ninx.Communication;
using ninx.Communication;

namespace ninx.Application.Services
{
    public interface ILoginService
    {
        public Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
