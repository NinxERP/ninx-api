using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings
{
    public class CargoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Cargo, CargoResponse>()
                .Map(dest => dest.Permissoes, src => src.CargoPermissoes.Select(cp => cp.Permissao));
        }
    }
}
