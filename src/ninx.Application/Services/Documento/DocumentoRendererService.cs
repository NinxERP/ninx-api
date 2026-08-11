using System.Net;
using System.Text;
using iText.Html2pdf;
using ninx.Domain.Enums;
using ninx.Domain.Exceptions;
using ninx.Domain.Interfaces;

namespace ninx.Application.Services
{
    public class DocumentoRendererService : IDocumentoRendererService
    {
        private readonly IDocumentoTemplateRepository _documentoTemplateRepository;

        public DocumentoRendererService(IDocumentoTemplateRepository documentoTemplateRepository)
        {
            _documentoTemplateRepository = documentoTemplateRepository;
        }

        public async Task<string> RenderizarHtmlAsync(TipoDocumento tipoDocumento, Dictionary<string, string> tokens)
        {
            var template = await _documentoTemplateRepository.GetByTipoAsync(tipoDocumento);
            if (template == null)
                throw new NotFoundException($"Nenhum template ativo encontrado para o documento '{tipoDocumento}'.");

            var html = template.ConteudoHtml;

            foreach (var (chave, valor) in tokens)
            {
                var valorFinal = chave.StartsWith("Html.", StringComparison.Ordinal) ? valor : WebUtility.HtmlEncode(valor);
                html = html.Replace("{{" + chave + "}}", valorFinal);
            }

            return html;
        }

        public Task<string> ConverterParaPdfBase64Async(string html)
        {
            using var input = new MemoryStream(Encoding.UTF8.GetBytes(html));
            using var output = new MemoryStream();

            HtmlConverter.ConvertToPdf(input, output);

            return Task.FromResult(Convert.ToBase64String(output.ToArray()));
        }
    }
}
