using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Repositories
{
    public interface ILogAuditoriaRepository : IRepositoryBase<LogAuditoria>
    {
        Task<(IEnumerable<LogAuditoria> Data, int TotalCount)> GetPaginatedFiltradoAsync(int? comercioId, int? usuarioId, int pageNumber, int pageSize);
    }
}
