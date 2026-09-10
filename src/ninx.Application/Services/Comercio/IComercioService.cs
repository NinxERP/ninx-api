using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IComercioService
    {
        public Task<PaginatedResponse<ComercioResponse>> GetAll(PaginationRequest request, int usuarioIdLogado);
        public Task<IEnumerable<ComercioResponse>> GetByUsuarioId(int usuarioId, int usuarioIdLogado);
        public Task<ComercioResponse> GetByIdAsync(int id, int usuarioIdLogado);
        public Task<ComercioResponse> CriarAsync(ComercioRequest request, int usuarioIdLogado);
        public Task<ComercioResponse> AtualizarAsync(int id, int usuarioIdLogado, ComercioRequest request);
        public Task DesativarAsync(int id, int usuarioLogado);
    }
}
