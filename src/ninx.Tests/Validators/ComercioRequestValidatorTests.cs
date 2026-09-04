using FluentValidation.TestHelper;
using ninx.Application.Validators.Request;
using ninx.Communication;
using Xunit;

namespace ninx.Tests.Validators
{
    public class ComercioRequestValidatorTests
    {
        private readonly ComercioRequestValidator _validator = new();

        private static ComercioRequest RequestValido() => new() { Nome = "Comércio Teste" };

        [Fact]
        public void Validate_RequestValido_NaoDeveTerErros()
        {
            var result = _validator.TestValidate(RequestValido());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_NomeCurto_DeveTerErro()
        {
            var request = RequestValido();
            request.Nome = "AB";
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void Validate_CnpjComQuantidadeErradaDeDigitos_DeveTerErro()
        {
            var request = RequestValido();
            request.CNPJ = "123456";
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.CNPJ);
        }

        [Fact]
        public void Validate_CnpjValidoComMascara_NaoDeveTerErro()
        {
            var request = RequestValido();
            request.CNPJ = "12.345.678/0001-95";
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.CNPJ);
        }

        [Fact]
        public void Validate_AssinaturaBase64Invalida_DeveTerErro()
        {
            var request = RequestValido();
            request.AssinaturaResponsavelBase64 = "não é base64 válido!!!";
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.AssinaturaResponsavelBase64);
        }

        [Fact]
        public void Validate_LimiteCreditoPadraoZero_DeveTerErro()
        {
            var request = RequestValido();
            request.LimiteCreditoPadrao = 0;
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.LimiteCreditoPadrao);
        }
    }
}
