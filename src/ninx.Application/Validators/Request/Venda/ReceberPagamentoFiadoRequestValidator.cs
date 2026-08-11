using FluentValidation;
using ninx.Communication;
using ninx.Domain.Enums;

namespace ninx.Application.Validators.Request
{
    public class ReceberPagamentoFiadoRequestValidator : AbstractValidator<ReceberPagamentoFiadoRequest>
    {
        public ReceberPagamentoFiadoRequestValidator()
        {
            RuleFor(x => x.ValorPago)
                .GreaterThan(0).WithMessage("Valor pago deve ser maior que zero.");

            RuleFor(x => x.FormaPagamento)
                .Must(v => Enum.IsDefined(typeof(FormaPagamento), v)).WithMessage("Forma de pagamento inv�lida.");
        }
    }
}
