using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class AssinaturaEletronicaRepository : RepositoryBase<AssinaturaEletronica>, IAssinaturaEletronicaRepository
    {
        private readonly NinxDB _context;
        public AssinaturaEletronicaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }
        
        public async Task<AssinaturaEletronica?> GetByGuidAsync(Guid guid)
        {
            return await _context.AssinaturaEletronica
            .FirstOrDefaultAsync(a => a.DocumentoGuid == guid);
        }

        public async Task<AssinaturaEletronica?> GetClienteLojaAssinaturaByGuidAsync(Guid guid)
        {
            return await _context.AssinaturaEletronica
                    .Include(a => a.Venda)
                        .ThenInclude(v => v.Cliente)
                    .Include(a => a.Venda)
                        .ThenInclude(v => v.Comercio)
                    .Include(a => a.Venda) 
                        .ThenInclude(v => v.ItensVenda)
                    .FirstOrDefaultAsync(a => a.DocumentoGuid == guid);
        }

    }
}
