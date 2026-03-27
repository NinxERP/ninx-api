using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class UsuarioRepository : RepositoryBase<Usuario>, IUsuarioRepository
    {
        private readonly NinxDB _context;
        public UsuarioRepository(NinxDB context) : base(context)
        { 
            _context = context;
        }

        public async Task<Usuario?> GetUsuarioByEmail(string email)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario?> GetUsuarioAndUsuarioComercioById(int id)
        {
            return await _context.Usuarios.Include(u => u.UsuarioComercios)
                                          .FirstOrDefaultAsync(u => u.UsuarioID == id);
        }
    }
}
