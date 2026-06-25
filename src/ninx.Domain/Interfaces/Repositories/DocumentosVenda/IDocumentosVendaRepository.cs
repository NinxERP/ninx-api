using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IDocumentosVendaRepository : IRepositoryBase<DocumentosVenda>
    {
        Task<DocumentosVenda?> GetByGuidAsync(Guid guid);
        Task<DocumentosVenda?> GetClienteLojaAssinaturaByGuidAsync(Guid guid);

    }
}
