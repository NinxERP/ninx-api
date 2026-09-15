using FluentAssertions;
using ninx.Domain.Regras;
using Xunit;

namespace ninx.Tests.Regras
{
    public class VencimentoFiadoTests
    {
        [Theory]
        [InlineData("2026-09-05", 10, "2026-09-10")] // antes do dia: vence neste mês
        [InlineData("2026-09-10", 10, "2026-10-10")] // no próprio dia: mês seguinte
        [InlineData("2026-09-25", 10, "2026-10-10")] // depois do dia: mês seguinte
        [InlineData("2026-12-20", 5, "2027-01-05")]  // virada de ano
        [InlineData("2026-01-31", 31, "2026-02-28")] // mês curto: último dia
        [InlineData("2026-02-01", 30, "2026-02-28")]
        public void Calcular_DeveRetornarProximoDiaFixo(string venda, int dia, string esperado)
        {
            VencimentoFiado.Calcular(DateTime.Parse(venda), dia).Should().Be(DateTime.Parse(esperado));
        }
    }
}
