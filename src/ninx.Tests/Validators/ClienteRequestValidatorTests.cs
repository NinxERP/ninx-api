using FluentAssertions;
using FluentValidation.TestHelper;
using ninx.Application.Validators.Request;
using ninx.Communication;
using Xunit;

namespace ninx.Tests.Validators
{
    public class ClienteRequestValidatorTests
    {
        private readonly ClienteRequestValidator _validator = new();

        private static ClienteRequest RequestValido() => new()
        {
            Nome = "João da Silva",
            Cpf = "52998224725", // CPF válido
            EnderecoLogradouro = "Rua das Flores",
            EnderecoNumero = "10",
            EnderecoBairro = "Centro",
            EnderecoCidade = "São Paulo",
            EnderecoUF = "SP",
            EnderecoCEP = "01000000"
        };

        [Fact]
        public void Validate_RequestValido_NaoDeveTerErros()
        {
            var result = _validator.TestValidate(RequestValido());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_NomeVazio_DeveTerErro()
        {
            var request = RequestValido();
            request.Nome = "";
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void Validate_CpfInvalido_DeveTerErro()
        {
            var request = RequestValido();
            request.Cpf = "11111111111";
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Cpf);
        }

        [Fact]
        public void Validate_CepComMenosDe8Digitos_DeveTerErro()
        {
            var request = RequestValido();
            request.EnderecoCEP = "123";
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.EnderecoCEP);
        }

        [Fact]
        public void Validate_UfInvalida_DeveTerErro()
        {
            var request = RequestValido();
            request.EnderecoUF = "XX";
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.EnderecoUF);
        }

        [Fact]
        public void Validate_EmailInvalido_DeveTerErro()
        {
            var request = RequestValido();
            request.Email = "nao-e-email";
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_EmailNulo_NaoDeveTerErro()
        {
            var request = RequestValido();
            request.Email = null;
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_LimiteCreditoZeroOuNegativo_DeveTerErro()
        {
            var request = RequestValido();
            request.LimiteCredito = 0;
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.LimiteCredito);
        }

        [Fact]
        public void Validate_LimiteCreditoNulo_NaoDeveTerErro()
        {
            var request = RequestValido();
            request.LimiteCredito = null;
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.LimiteCredito);
        }
    }
}
