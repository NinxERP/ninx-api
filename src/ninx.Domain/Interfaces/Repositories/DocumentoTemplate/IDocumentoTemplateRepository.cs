using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Domain.Interfaces
{
    public interface IDocumentoTemplateRepository : IRepositoryBase<DocumentoTemplate>
    {
        Task<DocumentoTemplate?> GetByTipoAsync(TipoDocumento tipoDocumento);
    }
}
