using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Repositories.Comercio;
using ninx.Infra.Repository.Base;

namespace ninx.Infra.Repository.Comercio
{
    public class ComercioRepository : RepositoryBase<Domain.Entities.Comercio>, IComercioRepository
    {
        private readonly AppDbContext _context;
        public ComercioRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }
            public async Task<IEnumerable<Domain.Entities.Comercio>> GetByUsuarioId(int usuarioId)
        {
            return await _context.Comercio.Include(c => c.UsuarioComercios).Where(c => c.UsuarioComercios.Any(uc => uc.UsuarioID == usuarioId)).ToListAsync();
        }
    }
}
