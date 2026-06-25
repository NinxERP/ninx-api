using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class DocumentosVendaRepository : RepositoryBase<DocumentosVenda>, IDocumentosVendaRepository
    {
        private readonly NinxDB _context;
        public DocumentosVendaRepository(NinxDB context) : base(context)
        {
            _context = context;
        }
        
        public async Task<DocumentosVenda?> GetByGuidAsync(Guid guid)
        {
            return await _context.DocumentosVenda
            .FirstOrDefaultAsync(a => a.DocumentoGuid == guid);
        }

        public async Task<DocumentosVenda?> GetClienteLojaAssinaturaByGuidAsync(Guid guid)
        {
            return await _context.DocumentosVenda
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
