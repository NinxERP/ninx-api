using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings
{
    public class EstoqueMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Estoque, EstoqueResponse>();
        }
    }
}
