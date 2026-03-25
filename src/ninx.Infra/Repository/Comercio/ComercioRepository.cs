using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories;

namespace ninx.Infra.Repository
{
    public class ComercioRepository : RepositoryBase<Comercio>, IComercioRepository
    {
        private readonly NinxDB _context;
        public ComercioRepository(NinxDB context) : base(context)
        {
            _context = context;
        }
            public async Task<IEnumerable<Comercio>> GetByUsuarioId(int usuarioId)
        {
            return await _context.Comercio.Include(c => c.UsuarioComercios).Where(c => c.UsuarioComercios.Any(uc => uc.UsuarioID == usuarioId)).ToListAsync();
        }
    }
}
