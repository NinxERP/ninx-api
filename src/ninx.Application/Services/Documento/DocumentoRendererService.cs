using System.Net;
using System.Text;
using iText.Html2pdf;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Utils;
using ninx.Communication;
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

        public string AnexarPdfBase64(string pdfBase64, string anexoPdfBase64)
        {
            using var output = new MemoryStream();

            using (var destino = new PdfDocument(new PdfWriter(output)))
            {
                var merger = new PdfMerger(destino);
                foreach (var origemBase64 in new[] { pdfBase64, anexoPdfBase64 })
                {
                    using var origem = new PdfDocument(new PdfReader(new MemoryStream(Convert.FromBase64String(origemBase64))));
                    merger.Merge(origem, 1, origem.GetNumberOfPages());
                }
            }

            return Convert.ToBase64String(output.ToArray());
        }

        // Mesma cor da caneta exibida no portal.
        private static readonly Color CorTraco = new DeviceRgb(0.102f, 0.102f, 0.094f);

        public string AplicarTracosAssinatura(string pdfBase64, IReadOnlyCollection<TracoAssinaturaRequest> tracos)
        {
            using var output = new MemoryStream();

            using (var pdf = new PdfDocument(new PdfReader(new MemoryStream(Convert.FromBase64String(pdfBase64))), new PdfWriter(output)))
            {
                if (tracos.Any(t => t.Pagina > pdf.GetNumberOfPages()))
                    throw new BadRequestException("A assinatura aponta para uma página que o documento não tem.");

                foreach (var porPagina in tracos.Where(t => t.Pontos.Count >= 2).GroupBy(t => t.Pagina))
                {
                    var pagina = pdf.GetPage(porPagina.Key);
                    var area = pagina.GetPageSize();
                    var canvas = new PdfCanvas(pagina)
                        .SaveState()
                        .SetStrokeColor(CorTraco)
                        .SetLineCapStyle(PdfCanvasConstants.LineCapStyle.ROUND)
                        .SetLineJoinStyle(PdfCanvasConstants.LineJoinStyle.ROUND);

                    // Os pontos vêm com origem no topo da página; no PDF a origem fica embaixo.
                    foreach (var traco in porPagina)
                    {
                        canvas.SetLineWidth((float)traco.Espessura);
                        var primeiro = traco.Pontos[0];
                        canvas.MoveTo(area.GetLeft() + primeiro.X * area.GetWidth(), area.GetTop() - primeiro.Y * area.GetHeight());
                        foreach (var ponto in traco.Pontos.Skip(1))
                            canvas.LineTo(area.GetLeft() + ponto.X * area.GetWidth(), area.GetTop() - ponto.Y * area.GetHeight());
                        canvas.Stroke();
                    }

                    canvas.RestoreState();
                }
            }

            return Convert.ToBase64String(output.ToArray());
        }
    }
}
