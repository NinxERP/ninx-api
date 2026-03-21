using ninx.Communication.Request.Comercio;
using ninx.Communication.Response.Comercio;

namespace ninx.Application.Interfaces.Services.Comercio
{
    public interface IComercioService
    {
        public Task<IEnumerable<ComercioResponse>> GetAll();
        public Task<IEnumerable<ComercioResponse>> GetByUsuarioId(int usuarioId);
        public Task<ComercioResponse> GetByIdAsync(int id);
        public Task<ComercioResponse> CriarAsync(ComercioRequest request);
        public Task<ComercioResponse> AtualizarAsync(int id, ComercioRequest request);
        public Task DesativarAsync(int id);
    }
}
