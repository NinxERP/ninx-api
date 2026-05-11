using ninx.Communication;
using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IComercioService
    {
        public Task<IEnumerable<ComercioResponse>> GetAll();
        public Task<IEnumerable<ComercioResponse>> GetByUsuarioId(int usuarioId);
        public Task<ComercioResponse> GetByIdAsync(int id);
        public Task<ComercioResponse> CriarAsync(ComercioRequest request);
        public Task<ComercioResponse> AtualizarAsync(int id, int usuarioIdLogado, ComercioRequest request);
        public Task DesativarAsync(int id, int usuarioLogado);
    }
}
