using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
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

        public async Task<bool> ExisteAssinaturaPendenteAsync(int vendaId)
        {
            return await _context.AssinaturaEletronica
                .AsNoTracking()
                .AnyAsync(a => a.VendaID == vendaId && a.Assinado == false);
        }

        public async Task<List<Guid>> GetDocumentoGuidsPorVendaAsync(int vendaId)
        {
            return await _context.AssinaturaEletronica
                .AsNoTracking()
                .Where(a => a.VendaID == vendaId && a.DocumentoGuid != Guid.Empty)
                .Select(a => a.DocumentoGuid)
                .ToListAsync();
        }

        public async Task<int> CancelarPorVendaIdAsync(int vendaId, DateTime dataOperacao)
        {
            return await _context.AssinaturaEletronica
                .Where(a => a.VendaID == vendaId && a.Status != StatusAssinatura.Cancelada)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(a => a.Status, StatusAssinatura.Cancelada)
                    .SetProperty(a => a.AtualizadoEm, dataOperacao));
        }

        public async Task<AssinaturaEletronica?> GetByGuidAsync(Guid guid)
        {
            return await _context.AssinaturaEletronica
            .FirstOrDefaultAsync(a => a.DocumentoGuid == guid);
        }

        public async Task<List<AssinaturaEletronica>> GetAllByGuidAsync(Guid guid)
        {
            return await _context.AssinaturaEletronica
            .Where(a => a.DocumentoGuid == guid)
            .ToListAsync();
        }
        public async Task<AssinaturaEletronica?> GetByGuidParaAssinarAsync(Guid guid)
        {
            return await _context.AssinaturaEletronica
            .FirstOrDefaultAsync(a => a.DocumentoGuid == guid && a.Assinado != true);
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
