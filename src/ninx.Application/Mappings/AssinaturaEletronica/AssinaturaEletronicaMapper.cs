using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Application.Mappings
{
    public class AssinaturaEletronicaMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AssinaturaEletronica, AssinaturaEletronicaResponse>()
            .Map(dest => dest.Filename, src => BuildFilename(src.TipoDocumento, src.VendaID))
            .Map(dest => dest.DocumentoBase64, src => src.DocumentoOriginalBase64)
            .Map(dest => dest.DocumentoAssinadoBase64, src => src.DocumentoAssinadoBase64)
            .Map(dest => dest.AssinaturaBase64, src => src.ImagemAssinatura);
        }

        private static string BuildFilename(TipoDocumento? tipoDocumento, int vendaId)
        {
            var nomeDocumento = tipoDocumento switch
            {
                TipoDocumento.TermoCompromisso => "Termo de Compromisso",
                TipoDocumento.ReciboPagamentoParcial => "Recibo de Pagamento Parcial",
                TipoDocumento.ReciboQuitacaoGlobal => "Recibo de Quitação Global",
                _ => "Documento"
            };

            return $"{nomeDocumento} - Venda {vendaId}.pdf";
        }
    }
}
