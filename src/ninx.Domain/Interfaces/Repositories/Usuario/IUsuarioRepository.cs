using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IUsuarioRepository : IRepositoryBase<Usuario>
    {
        public Task<Usuario?> GetUsuarioByEmail(string email);
        public Task<Usuario?> GetUsuarioById(int id);
        public Task<Usuario?> GetByIdAndComercioIdAsync(int usuarioId, int comercioId);
        public Task<IEnumerable<Usuario>> GetAllByComercioIdAsync(int comercioId);

    }
}
