using FluentValidation;
using ninx.Communication.Venda;

namespace ninx.Application.Validators.Request
{
    public class PagamentoVendaRequestValidator : AbstractValidator<PagamentoVendaRequest>
    {
        public PagamentoVendaRequestValidator()
        {
            RuleFor(x => x.FormaPagamento)
                .GreaterThan(0).WithMessage("Forma de pagamento inválida.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
        }
    }
}
