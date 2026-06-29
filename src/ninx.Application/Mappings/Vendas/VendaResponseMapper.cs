using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings.Vendas
{
    internal class VendaResponseMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            TypeAdapterConfig<Venda, VendaResponse>
                .NewConfig()
                .Map(dest => dest.DocumentoGuid, src => src.AssinaturasEletronicas.Select(x => x.DocumentoGuid));
        }
    }
}
