using ninx.Communication.Request;
using ninx.Communication.Response;

namespace ninx.Application.Interfaces.Services
{
    public interface IUsuarioComercioService
    {
        Task<IEnumerable<UsuarioComercioResponse>> GetByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<UsuarioComercioResponse>> GetByComercioIdAsync(int comercioId);
        Task<UsuarioComercioResponse> CriarAsync(CriarUsuarioComercioRequest request);
        Task DesativarAsync(int usuarioId, int comercioId);
    }
}