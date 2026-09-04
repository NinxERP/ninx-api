using FluentValidation.TestHelper;
using ninx.Application.Validators.Request;
using ninx.Communication;
using ninx.Communication.Venda;
using Xunit;

namespace ninx.Tests.Validators
{
    public class CriarVendaRequestValidatorTests
    {
        private readonly CriarVendaRequestValidator _validator = new();

        private static CriarVendaRequest RequestValido() => new()
        {
            ComercioID = 1,
            UsuarioID = 1,
            ClienteID = 1,
            TipoVenda = 1,
            ItensVenda = new List<ItemVendaRequest> { new() { ProdutoID = 1, Quantidade = 1 } },
            Pagamentos = new List<PagamentoVendaRequest> { new() { FormaPagamento = 1, Valor = 10 } }
        };

        [Fact]
        public void Validate_RequestValido_NaoDeveTerErros()
        {
            var result = _validator.TestValidate(RequestValido());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_SemItens_DeveTerErro()
        {
            var request = RequestValido();
            request.ItensVenda = new List<ItemVendaRequest>();
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.ItensVenda);
        }

        [Fact]
        public void Validate_SemPagamentos_DeveTerErro()
        {
            var request = RequestValido();
            request.Pagamentos = new List<PagamentoVendaRequest>();
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Pagamentos);
        }

        [Fact]
        public void Validate_TipoVendaZero_DeveTerErro()
        {
            var request = RequestValido();
            request.TipoVenda = 0;
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.TipoVenda);
        }

        [Fact]
        public void Validate_ClienteIdZero_NaoDeveTerErro()
        {
            var request = RequestValido();
            request.ClienteID = 0;
            var result = _validator.TestValidate(request);
            result.ShouldNotHaveValidationErrorFor(x => x.ClienteID);
        }

        [Fact]
        public void Validate_ItemComQuantidadeZero_DeveTerErro()
        {
            var request = RequestValido();
            request.ItensVenda = new List<ItemVendaRequest> { new() { ProdutoID = 1, Quantidade = 0 } };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor("ItensVenda[0].Quantidade");
        }

        [Fact]
        public void Validate_PagamentoComValorZero_DeveTerErro()
        {
            var request = RequestValido();
            request.Pagamentos = new List<PagamentoVendaRequest> { new() { FormaPagamento = 1, Valor = 0 } };
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor("Pagamentos[0].Valor");
        }

        [Fact]
        public void Validate_ObservacoesMuitoLongas_DeveTerErro()
        {
            var request = RequestValido();
            request.Observacoes = new string('a', 501);
            var result = _validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.Observacoes);
        }
    }
}
