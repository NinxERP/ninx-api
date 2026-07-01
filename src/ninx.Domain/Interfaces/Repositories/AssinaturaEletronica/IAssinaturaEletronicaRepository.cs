using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IAssinaturaEletronicaRepository : IRepositoryBase<AssinaturaEletronica>
    {
        Task<AssinaturaEletronica?> GetByGuidParaAssinarAsync(Guid guid);
        Task<AssinaturaEletronica?> GetByGuidAsync(Guid guid);
        Task<AssinaturaEletronica?> GetClienteLojaAssinaturaByGuidAsync(Guid guid);

    }
}
