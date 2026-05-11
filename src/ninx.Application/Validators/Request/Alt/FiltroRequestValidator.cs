using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class FiltroRequestValidator : AbstractValidator<FiltroRequest>
    {
        public FiltroRequestValidator()
        {
            RuleFor(x => x.inicio).LessThanOrEqualTo(x => x.fim).WithMessage("Data inicial deve ser menor ou igual à data final.");
            RuleFor(x => x.fim).GreaterThanOrEqualTo(x => x.inicio).WithMessage("Data final deve ser maior ou igual à data inicial.");
            RuleFor(x => x.usuarioID).GreaterThan(0).WithMessage("O ID do usuário deve ser maior que zero.");   
            RuleFor(x => x.comercioID).GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");
        }
    }
}
