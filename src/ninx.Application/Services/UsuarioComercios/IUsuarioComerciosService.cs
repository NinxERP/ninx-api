using ninx.Communication.Request.UsuarioComercios;
using ninx.Communication.Response.UsuarioComercio;

namespace ninx.Domain.Interfaces.Services
{
    public interface IUsuarioComercioService
    {
        Task<IEnumerable<UsuarioComercioResponse>> GetByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<UsuarioComercioResponse>> GetByComercioIdAsync(int comercioId);
        Task<UsuarioComercioResponse> CriarAsync(CriarUsuarioComercioRequest request);
        Task DesativarAsync(int usuarioId, int comercioId);
    }
}