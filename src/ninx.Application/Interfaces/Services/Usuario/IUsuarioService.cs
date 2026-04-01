using ninx.Communication.Request;
using ninx.Communication.Response;

namespace ninx.Domain.Interfaces.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponse?> GetByIdAsync(int id);
        Task<UsuarioResponse> CriarAsync(
                   CriarUsuarioRequest request,
                   int executorId,
                   string executorRole,
                   int? executorComercioId);
        Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request);
        Task DesativarAsync(int id);
    }
}