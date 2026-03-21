using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories.Base;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IUsuarioComercioRepository : IRepositoryBase<UsuarioComercio>
    {
        Task<IEnumerable<UsuarioComercio>> GetByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<UsuarioComercio>> GetByComercioIdAsync(int comercioId);
        Task<UsuarioComercio?> GetByUsuarioIdAndComercioIdAsync(int usuarioId, int comercioId);
    }
}