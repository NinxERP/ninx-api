using Microsoft.EntityFrameworkCore;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Interfaces;

namespace ninx.Infra.Repository
{
    public class DocumentoTemplateRepository : RepositoryBase<DocumentoTemplate>, IDocumentoTemplateRepository
    {
        private readonly NinxDB _context;
        public DocumentoTemplateRepository(NinxDB context) : base(context)
        {
            _context = context;
        }

        public async Task<DocumentoTemplate?> GetByTipoAsync(TipoDocumento tipoDocumento)
        {
            return await _context.DocumentosTemplate
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TipoDocumento == tipoDocumento && t.Ativo);
        }
    }
}
