using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings.Estoque
{
    public class EstoqueMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Estoque, EstoqueResponse>();
        }
    }
}
