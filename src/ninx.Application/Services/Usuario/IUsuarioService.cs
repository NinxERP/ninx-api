using ninx.Communication;
using ninx.Communication;
using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponse> GetByIdAndComercioIdAsync(int id, int comercioId);
        Task<UsuarioResponse> GetById(int id, int usuarioIdLogado);
        Task<UsuarioResponse> GetAll(int usuarioIdLogado);
        Task<UsuarioResponse> GetAllByComercioId(int comercioId);
        Task<UsuarioResponse> CriarAsync(
                   CriarUsuarioRequest request,
                   int executorId,
                   Permissao permissao,
                   int? executorComercioId);
        Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId);
        Task DesativarAsync(int id, int comercioId);
    }
}