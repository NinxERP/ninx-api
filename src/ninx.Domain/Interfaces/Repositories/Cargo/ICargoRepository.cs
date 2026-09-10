using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface ICargoRepository : IRepositoryBase<Cargo>
    {
        Task<IEnumerable<Cargo>> GetDisponiveisParaComercioAsync(int comercioId);
        Task<IEnumerable<Cargo>> GetCargosBaseAsync();
        Task<bool> ExisteNomeAsync(string nome, int? comercioId);
        Task<Cargo?> GetCargoAdminAsync();
    }
}
