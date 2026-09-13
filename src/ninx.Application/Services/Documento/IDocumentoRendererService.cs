using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IDocumentoRendererService
    {
        Task<string> RenderizarHtmlAsync(TipoDocumento tipoDocumento, Dictionary<string, string> tokens);
        Task<string> ConverterParaPdfBase64Async(string html);

        /// <summary>Devolve um PDF com as páginas de <paramref name="pdfBase64"/> seguidas das de <paramref name="anexoPdfBase64"/>.</summary>
        string AnexarPdfBase64(string pdfBase64, string anexoPdfBase64);
    }
}
