using ninx.Communication;
using ninx.Communication;
using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IUsuarioComercioService
    {
        Task<IEnumerable<UsuarioComercioResponse>> GetByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<UsuarioComercioResponse>> GetByComercioIdAsync(int comercioId);
        Task<UsuarioComercioResponse> CriarAsync(CriarUsuarioComercioRequest request);
        Task<UsuarioComercioResponse> AtualizarAsync(AtualizarUsuarioComercioRequest request, Permissao usuarioLogadoPermissao);
        Task DesativarAsync(int usuarioId, int comercioId);
    }
}

