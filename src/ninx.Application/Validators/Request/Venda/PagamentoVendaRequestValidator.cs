using FluentValidation;
using ninx.Communication.Venda;
using ninx.Domain.Enums;

namespace ninx.Application.Validators.Request
{
    public class PagamentoVendaRequestValidator : AbstractValidator<PagamentoVendaRequest>
    {
        public PagamentoVendaRequestValidator()
        {
            RuleFor(x => x.FormaPagamento)
                .Must(v => Enum.IsDefined(typeof(FormaPagamento), v)).WithMessage("Forma de pagamento inv�lida.");

            RuleFor(x => x.Valor)
                .GreaterThanOrEqualTo(0).WithMessage("Valor não pode ser negativo.");
        }
    }
}
