using ninx.Communication.Request.Usuario;
using ninx.Communication.Response.Usuario;

namespace ninx.Domain.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponse?> GetByIdAsync(int id);
        Task<UsuarioResponse> CriarAsync(CriarUsuarioRequest request);
        Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request);
        Task DesativarAsync(int id);
    }
}