using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class FiltroRequestValidator : AbstractValidator<FiltroRequest>
    {
        public FiltroRequestValidator()
        {
            RuleFor(x => x.inicio).LessThanOrEqualTo(x => x.fim).WithMessage("Data inicial deve ser menor ou igual � data final.");
            RuleFor(x => x.fim).GreaterThanOrEqualTo(x => x.inicio).WithMessage("Data final deve ser maior ou igual � data inicial.");
        }
    }
}
