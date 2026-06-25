using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings
{
    public class DocumentosVendaMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<DocumentosVenda, DocumentosVendaResponse>()
            .Map(dest => dest.DocumentoBase64, src => src.ImagemAssinatura);
        }
    }
}
