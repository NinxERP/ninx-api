using ninx.Domain.Entities;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class CargoEfetivoService : ICargoEfetivoService
    {
        private readonly ICargoRepository _cargoRepository;

        public CargoEfetivoService(ICargoRepository cargoRepository)
        {
            _cargoRepository = cargoRepository;
        }

        public async Task<Cargo> ResolverCargoEfetivoAsync(Usuario usuario, Cargo? cargoDoVinculo)
        {
            if (usuario.Admin)
            {
                var cargoAdmin = await _cargoRepository.GetCargoAdminAsync();
                if (cargoAdmin is null)
                    throw new InvalidOperationException("Cargo 'Admin' não configurado na base de dados.");

                return cargoAdmin;
            }

            if (cargoDoVinculo is null)
                throw new UnauthorizedException("Você não tem acesso a este comércio.");

            return cargoDoVinculo;
        }
    }
}
