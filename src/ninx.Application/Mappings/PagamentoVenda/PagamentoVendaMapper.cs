using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings.PagamentoVenda
{
    public class PagamentoVendaMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<PagamentoVenda, PagamentoVendaResponse>();
        }
    }
}
