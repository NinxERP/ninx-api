using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ReceberPagamentoFiadoRequestValidator : AbstractValidator<ReceberPagamentoFiadoRequest>
    {
        public ReceberPagamentoFiadoRequestValidator()
        {
            RuleFor(x => x.ValorPago)
                .GreaterThan(0).WithMessage("Valor pago deve ser maior que zero.");

            RuleFor(x => x.FormaPagamento)
                .GreaterThan(0).WithMessage("Forma de pagamento inválida.");
        }
    }
}
