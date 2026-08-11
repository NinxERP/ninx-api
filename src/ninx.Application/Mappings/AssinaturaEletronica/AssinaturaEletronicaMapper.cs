using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings
{
    public class AssinaturaEletronicaMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AssinaturaEletronica, AssinaturaEletronicaResponse>()
            .Map(dest => dest.DocumentoBase64, src => src.DocumentoOriginalBase64)
            .Map(dest => dest.AssinaturaBase64, src => src.ImagemAssinatura);
        }
    }
}
