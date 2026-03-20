using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Infra.Repository.Base;

namespace ninx.Infra.Repository.Usuario
{
    public class UsuarioRepository : RepositoryBase<Domain.Entities.Usuario>
    {
        private readonly AppDbContext _context;
        public UsuarioRepository(AppDbContext context) : base(context)
        { 
            _context = context;
        }

        public async Task<Domain.Entities.Usuario?> GetUsuarioAndUsurioComercioByEmail(string email)
        {
            return await _context.Usuarios.Include(u => u.UsuarioComercios)
                                          .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
