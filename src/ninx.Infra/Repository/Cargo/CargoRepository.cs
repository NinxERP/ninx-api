using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Constants;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class CargoRepository : RepositoryBase<Cargo>, ICargoRepository
    {
        private readonly NinxDB _context;

        public CargoRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cargo>> GetDisponiveisParaComercioAsync(int comercioId)
        {
            return await _context.Cargos
                .AsNoTracking()
                .Include(x => x.CargoPermissoes).ThenInclude(cp => cp.Permissao)
                .Where(x => x.Ativo && !x.Reservado && (x.ComercioID == null || x.ComercioID == comercioId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Cargo>> GetCargosBaseAsync()
        {
            return await _context.Cargos
                .AsNoTracking()
                .Include(x => x.CargoPermissoes).ThenInclude(cp => cp.Permissao)
                .Where(x => x.ComercioID == null)
                .ToListAsync();
        }

        public async Task<bool> ExisteNomeAsync(string nome, int? comercioId)
        {
            return await _context.Cargos
                .AsNoTracking()
                .AnyAsync(x => x.Nome == nome && x.ComercioID == comercioId);
        }

        public async Task<Cargo?> GetCargoAdminAsync()
        {
            return await _context.Cargos
                .AsNoTracking()
                .Include(x => x.CargoPermissoes).ThenInclude(cp => cp.Permissao)
                .FirstOrDefaultAsync(x => x.ComercioID == null && x.Nome == CargoConstantes.NomeAdmin);
        }

        public async Task<Cargo?> GetComPermissoesAsync(int cargoId)
        {
            return await _context.Cargos
                .Include(x => x.CargoPermissoes).ThenInclude(cp => cp.Permissao)
                .FirstOrDefaultAsync(x => x.CargoID == cargoId);
        }
    }
}
