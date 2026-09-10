using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IDocumentoRendererService
    {
        Task<string> RenderizarHtmlAsync(TipoDocumento tipoDocumento, Dictionary<string, string> tokens);
        Task<string> ConverterParaPdfBase64Async(string html);
    }
}
