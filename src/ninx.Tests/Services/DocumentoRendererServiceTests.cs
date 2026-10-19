using FluentAssertions;
using iText.Kernel.Pdf;
using ninx.Application.Services;
using ninx.Communication;
using ninx.Domain.Exceptions;
using Xunit;

namespace ninx.Tests.Services
{
    public class DocumentoRendererServiceTests
    {
        private readonly DocumentoRendererService _service = new(null!);

        private static List<TracoAssinaturaRequest> TracoNaPagina(int pagina) => new()
        {
            new() { Pagina = pagina, Espessura = 1.5, Pontos = new() { new() { X = 0.1, Y = 0.8 }, new() { X = 0.3, Y = 0.85 } } }
        };

        [Fact]
        public async Task AplicarTracosAssinatura_DeveDesenharSobreOOriginalSemMudarAsPaginas()
        {
            var original = await _service.ConverterParaPdfBase64Async("<p>termo</p>");

            var assinado = _service.AplicarTracosAssinatura(original, TracoNaPagina(1));

            assinado.Should().NotBe(original);
            using var pdf = new PdfDocument(new PdfReader(new MemoryStream(Convert.FromBase64String(assinado))));
            pdf.GetNumberOfPages().Should().Be(1);
            var conteudo = System.Text.Encoding.ASCII.GetString(pdf.GetPage(1).GetContentBytes());
            conteudo.Should().Contain(" l").And.Contain(" m");
        }

        [Fact]
        public async Task AplicarTracosAssinatura_PaginaInexistente_DeveLancarBadRequest()
        {
            var original = await _service.ConverterParaPdfBase64Async("<p>termo</p>");

            var act = () => _service.AplicarTracosAssinatura(original, TracoNaPagina(2));

            act.Should().Throw<BadRequestException>();
        }
    }
}
