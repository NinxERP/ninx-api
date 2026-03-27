using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface IUsuarioRepository : IRepositoryBase<Usuario>
    {
        public Task<Usuario?> GetUsuarioByEmail(string email);
        public Task<Usuario?> GetUsuarioAndUsuarioComercioById(int id);
    }
}
