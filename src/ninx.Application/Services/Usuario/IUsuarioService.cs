using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponse> GetByIdAndComercioIdAsync(int id, int comercioId, int usuarioIdLogado, int pesoLogado);
        Task<UsuarioResponse> GetById(int id, int usuarioIdLogado);
        Task<PaginatedResponse<UsuarioResponse>> GetAll(int usuarioIdLogado, PaginationRequest request);
        Task<PaginatedResponse<UsuarioResponse>> GetAllByComercioId(int comercioId, int usuarioIdLogado, int pesoLogado, PaginationRequest request);
        Task<UsuarioResponse> BuscarPorEmailAsync(string email, int usuarioIdLogado, int pesoLogado);
        Task<UsuarioResponse> CriarAsync(
                   CriarUsuarioRequest request,
                   int executorId,
                   int pesoLogado,
                   int? executorComercioId);
        Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId, int usuarioIdLogado, int pesoLogado);
        Task<UsuarioResponse> AtualizarGlobalAsync(int id, AtualizarUsuarioRequest request, int usuarioIdLogado);
        Task DesativarAsync(int id, int comercioId, int usuarioIdLogado);
        Task DesativarGlobalAsync(int id, int usuarioIdLogado);
        Task ResetarSenhaAsync(int id, int usuarioIdLogado, ResetarSenhaRequest request);
        Task<UsuarioResponse> AtualizarAdminAsync(int id, int usuarioIdLogado, AtualizarAdminRequest request);
    }
}
