using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings.AssinaturaPlano
{
    public class AssinaturaPlanoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AssinaturaPlano, AssinaturaPlanoResponse>();
        }
    }
}
