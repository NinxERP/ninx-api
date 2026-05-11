using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IUsuarioComercioRepository : IRepositoryBase<UsuarioComercio>
    {
        Task<IEnumerable<UsuarioComercio>> GetByUsuarioIdAsync(int usuarioId);
        Task<IEnumerable<UsuarioComercio>> GetByComercioIdAsync(int comercioId);
    }
}