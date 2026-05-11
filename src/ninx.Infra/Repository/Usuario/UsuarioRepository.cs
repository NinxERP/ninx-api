using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

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

        public async Task<Usuario?> GetUsuarioById(int usuarioID)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.UsuarioID == usuarioID);
        }


        public async Task<IEnumerable<Usuario>> GetAllByComercioIdAsync(int comercioId)
        {
            return await _context.Usuarios.AsNoTracking()
                .Include(x => x.UsuarioComercios)
                .Where(x => x.UsuarioComercios.Any(uc => uc.ComercioID == comercioId))
                .ToListAsync();
        }

        public async Task<Usuario?> GetByIdAndComercioIdAsync(int usuarioId, int comercioId)
        {
            return await _context.Usuarios
                .AsNoTracking()
                .Include(x => x.UsuarioComercios)
                .FirstOrDefaultAsync(x =>
                    x.UsuarioID == usuarioId &&
                    x.UsuarioComercios.Any(uc => uc.ComercioID == comercioId));
        }
    }
}
