using ninx.Communication;
using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IUsuarioComercioService
    {
        Task<IEnumerable<UsuarioComercioResponse>> GetByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<UsuarioComercioResponse>> GetByComercioIdAsync(int comercioId);
        Task<UsuarioComercioResponse> CriarAsync(CriarUsuarioComercioRequest request, int usuarioLogadoId);
        Task<UsuarioComercioResponse> AtualizarAsync(AtualizarUsuarioComercioRequest request, int usuarioLogadoId);
        Task DesativarAsync(int usuarioId, int comercioId, int usuarioLogadoId);
    }
}

