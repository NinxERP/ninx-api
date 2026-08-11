using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class RelatorioClientesInativosRequestValidator : AbstractValidator<RelatorioClientesInativosRequest>
    {
        public RelatorioClientesInativosRequestValidator()
        {
            RuleFor(x => x.DiasSemComprar)
                .GreaterThan(0).WithMessage("Dias sem comprar deve ser maior que zero.");
        }
    }
}
