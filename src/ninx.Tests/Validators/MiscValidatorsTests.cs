using FluentValidation.TestHelper;
using ninx.Application.Validators.Request;
using ninx.Communication;
using ninx.Communication.Venda;
using Xunit;

namespace ninx.Tests.Validators
{
    public class ItemVendaRequestValidatorTests
    {
        private readonly ItemVendaRequestValidator _validator = new();

        [Fact]
        public void Validate_ProdutoIdZero_DeveTerErro()
        {
            var result = _validator.TestValidate(new ItemVendaRequest { ProdutoID = 0, Quantidade = 1 });
            result.ShouldHaveValidationErrorFor(x => x.ProdutoID);
        }

        [Fact]
        public void Validate_QuantidadeZero_DeveTerErro()
        {
            var result = _validator.TestValidate(new ItemVendaRequest { ProdutoID = 1, Quantidade = 0 });
            result.ShouldHaveValidationErrorFor(x => x.Quantidade);
        }

        [Fact]
        public void Validate_Valido_NaoDeveTerErros()
        {
            var result = _validator.TestValidate(new ItemVendaRequest { ProdutoID = 1, Quantidade = 2 });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    public class PagamentoVendaRequestValidatorTests
    {
        private readonly PagamentoVendaRequestValidator _validator = new();

        [Fact]
        public void Validate_FormaPagamentoInvalida_DeveTerErro()
        {
            var result = _validator.TestValidate(new PagamentoVendaRequest { FormaPagamento = 999, Valor = 10 });
            result.ShouldHaveValidationErrorFor(x => x.FormaPagamento);
        }

        [Fact]
        public void Validate_ValorZero_NaoDeveTerErro()
        {
            var result = _validator.TestValidate(new PagamentoVendaRequest { FormaPagamento = 1, Valor = 0 });
            result.ShouldNotHaveValidationErrorFor(x => x.Valor);
        }

        [Fact]
        public void Validate_ValorNegativo_DeveTerErro()
        {
            var result = _validator.TestValidate(new PagamentoVendaRequest { FormaPagamento = 1, Valor = -1 });
            result.ShouldHaveValidationErrorFor(x => x.Valor);
        }

        [Fact]
        public void Validate_Valido_NaoDeveTerErros()
        {
            var result = _validator.TestValidate(new PagamentoVendaRequest { FormaPagamento = 1, Valor = 10 });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }

    public class CriarCargoRequestValidatorTests
    {
        private readonly CriarCargoRequestValidator _validator = new();

        [Fact]
        public void Validate_NomeVazio_DeveTerErro()
        {
            var result = _validator.TestValidate(new CriarCargoRequest { Nome = "", PermissaoIds = [1] });
            result.ShouldHaveValidationErrorFor(x => x.Nome);
        }

        [Fact]
        public void Validate_PermissaoIdZero_DeveTerErro()
        {
            var result = _validator.TestValidate(new CriarCargoRequest { Nome = "Gerente", PermissaoIds = [0] });
            result.ShouldHaveValidationErrorFor("PermissaoIds[0]");
        }

        [Fact]
        public void Validate_ComercioIdZero_DeveTerErro()
        {
            var result = _validator.TestValidate(new CriarCargoRequest { Nome = "Gerente", PermissaoIds = [1], ComercioID = 0 });
            result.ShouldHaveValidationErrorFor(x => x.ComercioID);
        }

        [Fact]
        public void Validate_ComercioIdNulo_NaoDeveTerErro()
        {
            var result = _validator.TestValidate(new CriarCargoRequest { Nome = "Gerente", PermissaoIds = [1], ComercioID = null });
            result.ShouldNotHaveValidationErrorFor(x => x.ComercioID);
        }

        [Fact]
        public void Validate_Valido_NaoDeveTerErros()
        {
            var result = _validator.TestValidate(new CriarCargoRequest { Nome = "Gerente", PermissaoIds = [1], ComercioID = 1 });
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
