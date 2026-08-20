using ninx.Domain.Entities;

namespace ninx.Application.Services
{
    public interface ICargoEfetivoService
    {
        /// <summary>
        /// Resolve o cargo a usar nas claims do JWT: administradores de plataforma sempre carregam o
        /// cargo reservado "Admin", independente de terem ou não vínculo real no comércio.
        /// </summary>
        Task<Cargo> ResolverCargoEfetivoAsync(Usuario usuario, Cargo? cargoDoVinculo);
    }
}
