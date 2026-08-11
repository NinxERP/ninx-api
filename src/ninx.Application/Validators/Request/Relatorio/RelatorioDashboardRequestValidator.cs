using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class RelatorioDashboardRequestValidator : AbstractValidator<RelatorioDashboardRequest>
    {
        public RelatorioDashboardRequestValidator()
        {
            RuleFor(x => x.Inicio)
                .LessThanOrEqualTo(x => x.Fim!.Value).WithMessage("Data inicial deve ser menor ou igual à data final.")
                .When(x => x.Inicio.HasValue && x.Fim.HasValue);

            RuleFor(x => x.Fim)
                .GreaterThanOrEqualTo(x => x.Inicio!.Value).WithMessage("Data final deve ser maior ou igual à data inicial.")
                .When(x => x.Inicio.HasValue && x.Fim.HasValue);
        }
    }
}
