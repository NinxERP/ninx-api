using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface IAssinaturaPlanoRepository : IRepositoryBase<AssinaturaPlano>
    {
        Task<AssinaturaPlano?> GetByComercioIdAsync(int comercioId);
    }
}
