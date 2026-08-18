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
        Task<UsuarioResponse> BuscarPorEmailAsync(string email, Permissao permissaoLogado);
        Task<UsuarioResponse> CriarAsync(
                   CriarUsuarioRequest request,
                   int executorId,
                   Permissao permissao,
                   int? executorComercioId);
        Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId, int usuarioIdLogado, Permissao permissaoLogado);
        Task DesativarAsync(int id, int comercioId, int usuarioIdLogado);
        Task DesativarGlobalAsync(int id, int usuarioIdLogado);
        Task ResetarSenhaAsync(int id, int usuarioIdLogado, ResetarSenhaRequest request);
        Task<UsuarioResponse> AtualizarAdminAsync(int id, int usuarioIdLogado, AtualizarAdminRequest request);
    }
}