using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IAssinaturaEletronicaRepository : IRepositoryBase<AssinaturaEletronica>
    {
        Task<AssinaturaEletronica?> GetByGuidParaAssinarAsync(Guid guid);
        Task<AssinaturaEletronica?> GetByGuidAsync(Guid guid);
        Task<List<AssinaturaEletronica>> GetAllByGuidAsync(Guid guid);
        Task<AssinaturaEletronica?> GetClienteLojaAssinaturaByGuidAsync(Guid guid);
        Task<bool> ExisteAssinaturaPendenteAsync(int vendaId);
        Task<List<VendaDocumentoResumo>> GetDocumentosPorVendaIdsAsync(IEnumerable<int> vendaIds);
        Task<int> CancelarPorVendaIdAsync(int vendaId, DateTime dataOperacao);
    }
}
