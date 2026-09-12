using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IUsuarioService
    {
        Task<UsuarioResponse> GetByIdAndComercioIdAsync(int id, int comercioId, int usuarioIdLogado, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado);
        Task<UsuarioResponse> GetById(int id, int usuarioIdLogado);
        Task<PaginatedResponse<UsuarioListaResponse>> GetAll(int usuarioIdLogado, PaginationRequest request);
        Task<PaginatedResponse<UsuarioListaResponse>> GetAllByComercioId(int comercioId, int usuarioIdLogado, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado, PaginationRequest request);
        Task<UsuarioResponse> BuscarPorEmailAsync(string email, int usuarioIdLogado, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado);
        Task<UsuarioResponse> CriarAsync(
                   CriarUsuarioRequest request,
                   int executorId,
                   bool ehProprietarioLogado,
                   IEnumerable<string> permissoesLogado,
                   int? executorComercioId);
        Task<UsuarioResponse> AtualizarAsync(int id, AtualizarUsuarioRequest request, int comercioId, int usuarioIdLogado, bool ehProprietarioLogado, IEnumerable<string> permissoesLogado);
        Task<UsuarioResponse> AtualizarGlobalAsync(int id, AtualizarUsuarioRequest request, int usuarioIdLogado);
        Task DesativarAsync(int id, int comercioId, int usuarioIdLogado);
        Task DesativarGlobalAsync(int id, int usuarioIdLogado);
        Task ResetarSenhaAsync(int id, int usuarioIdLogado, ResetarSenhaRequest request);
    }
}
