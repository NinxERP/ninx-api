using ninx.Domain.Interfaces.Repositories.Base;

namespace ninx.Domain.Interfaces.Repositories.Usuario
{
    public interface IUsuarioRepository : IRepositoryBase<Entities.Usuario>
    {
        public Task<Domain.Entities.Usuario?> GetUsuarioAndUsuarioComercioByEmail(string email);
        public Task<Domain.Entities.Usuario?> GetUsuarioAndUsuarioComercioById(int id);
    }
}
