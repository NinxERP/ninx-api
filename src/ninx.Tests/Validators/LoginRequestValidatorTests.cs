using FluentValidation.TestHelper;
using ninx.Application.Validators.Request;
using ninx.Communication;
using Xunit;

namespace ninx.Tests.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator = new();

        [Fact]
        public void Validate_RequestValido_NaoDeveTerErros()
        {
            var result = _validator.TestValidate(new LoginRequest { Email = "usuario@teste.com", Senha = "senha123" });
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_EmailVazio_DeveTerErro()
        {
            var result = _validator.TestValidate(new LoginRequest { Email = "", Senha = "senha123" });
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_EmailSemFormatoValido_DeveTerErro()
        {
            var result = _validator.TestValidate(new LoginRequest { Email = "nao-e-email", Senha = "senha123" });
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Validate_SenhaCurta_DeveTerErro()
        {
            var result = _validator.TestValidate(new LoginRequest { Email = "usuario@teste.com", Senha = "123" });
            result.ShouldHaveValidationErrorFor(x => x.Senha);
        }

        [Fact]
        public void Validate_ComercioIdZeroOuNegativo_DeveTerErro()
        {
            var result = _validator.TestValidate(new LoginRequest { Email = "usuario@teste.com", Senha = "senha123", ComercioID = 0 });
            result.ShouldHaveValidationErrorFor(x => x.ComercioID);
        }

        [Fact]
        public void Validate_ComercioIdNulo_NaoDeveTerErro()
        {
            var result = _validator.TestValidate(new LoginRequest { Email = "usuario@teste.com", Senha = "senha123", ComercioID = null });
            result.ShouldNotHaveValidationErrorFor(x => x.ComercioID);
        }
    }
}
