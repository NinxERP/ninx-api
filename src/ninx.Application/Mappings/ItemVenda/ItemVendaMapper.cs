using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings.ItemVenda
{
    public class ItemVendaMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ItemVenda, ItemVendaResponse>();
        }
    }
}
