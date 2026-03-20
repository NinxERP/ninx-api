using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Infra.Repository.Base;

namespace ninx.Infra.Repository.Usuario
{
    public class UsuarioRepository : RepositoryBase<Domain.Entities.Usuario>, Domain.Interfaces.Repositories.Usuario.IUsuarioRepository
    {
        private readonly AppDbContext _context;
        public UsuarioRepository(AppDbContext context) : base(context)
        { 
            _context = context;
        }

        public async Task<Domain.Entities.Usuario?> GetUsuarioAndUsuarioComercioByEmail(string email)
        {
            return await _context.Usuarios.Include(u => u.UsuarioComercios)
                                          .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Domain.Entities.Usuario?> GetUsuarioAndUsuarioComercioById(int id)
        {
            return await _context.Usuarios.Include(u => u.UsuarioComercios)
                                          .FirstOrDefaultAsync(u => u.UsuarioID == id);
        }
    }
}
