using ninx.Communication;
using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponse> GetByIdAndComercioIdAsync(int id, int comercioId, Permissao permissaoLogado);
        Task<UsuarioResponse> GetById(int id, int usuarioIdLogado);
        Task<PaginatedResponse<UsuarioResponse>> GetAll(int usuarioIdLogado, PaginationRequest request);
        Task<PaginatedResponse<UsuarioResponse>> GetAllByComercioId(int comercioId, Permissao permissaoLogado, PaginationRequest request);
        Task<UsuarioResponse> CriarAsync(
                   CriarUsuarioRequest request,
                   int executorId,
                   Permissao permissao,
                   int? executorComercioId);
        Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId, Permissao permissaoLogado);
        Task DesativarAsync(int id, int comercioId, Permissao permissaoLogado);
    }
}