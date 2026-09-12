using ninx.Communication;

namespace ninx.Application.Services
{
    public interface ICargoService
    {
        Task<IEnumerable<CargoResponse>> GetAsync(int? comercioId);
        Task<IEnumerable<PermissaoResponse>> GetPermissoesDisponiveisAsync();
        Task<CargoResponse> CriarAsync(CriarCargoRequest request, int usuarioLogadoId);
        Task<CargoResponse> AtualizarAsync(int id, AtualizarCargoRequest request, int usuarioLogadoId);
        Task DesativarAsync(int id, int usuarioLogadoId);
    }
}
