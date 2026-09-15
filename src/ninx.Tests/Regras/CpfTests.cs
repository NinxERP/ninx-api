using FluentAssertions;
using ninx.Application.Validators;
using Xunit;

namespace ninx.Tests.Regras
{
    public class CpfTests
    {
        [Theory]
        [InlineData("52998224725", true)]
        [InlineData("529.982.247-25", true)]
        [InlineData("111.444.777-35", true)]
        [InlineData("52998224724", false)]
        [InlineData("11111111111", false)]
        [InlineData("123", false)]
        [InlineData(null, false)]
        public void Valido_DeveConferirDigitosVerificadores(string? cpf, bool esperado)
        {
            Cpf.Valido(cpf).Should().Be(esperado);
        }
    }
}
