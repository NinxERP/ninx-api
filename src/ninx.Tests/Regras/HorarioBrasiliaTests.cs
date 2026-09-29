using FluentAssertions;
using ninx.Domain.Regras;
using Xunit;

namespace ninx.Tests.Regras
{
    public class HorarioBrasiliaTests
    {
        [Fact]
        public void Converter_VendaDeNoiteEmUtc_DeveVoltarParaODiaDeBrasilia()
        {
            // 21:07 do dia 28 em Brasília é 00:07 do dia 29 em UTC.
            var utc = new DateTime(2026, 9, 29, 0, 7, 13, DateTimeKind.Utc);

            HorarioBrasilia.Converter(utc).Should().Be(new DateTime(2026, 9, 28, 21, 7, 13));
        }

        [Theory]
        [InlineData(DateTimeKind.Utc)]
        [InlineData(DateTimeKind.Unspecified)] // o EF devolve o que gravou em UTC sem marcar o fuso
        public void Converter_NaoDependeDoKindDeEntrada(DateTimeKind kind)
        {
            var entrada = DateTime.SpecifyKind(new DateTime(2026, 9, 29, 0, 7, 13), kind);

            HorarioBrasilia.Converter(entrada).Should().Be(new DateTime(2026, 9, 28, 21, 7, 13));
        }

        [Fact]
        public void Formatar_DeveInformarDataHoraEDeslocamento()
        {
            var utc = new DateTime(2026, 9, 29, 0, 7, 13, DateTimeKind.Utc);

            HorarioBrasilia.Formatar(utc).Should().Be("28/09/2026 21:07:13 (UTC-03:00)");
        }

        [Fact]
        public void Converter_ViradaDeMes_DeveRecuarOMes()
        {
            var utc = new DateTime(2026, 10, 1, 1, 0, 0, DateTimeKind.Utc);

            HorarioBrasilia.Converter(utc).Should().Be(new DateTime(2026, 9, 30, 22, 0, 0));
        }
    }
}
