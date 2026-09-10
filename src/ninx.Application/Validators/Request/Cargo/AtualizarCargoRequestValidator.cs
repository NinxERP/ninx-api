using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class AtualizarCargoRequestValidator : AbstractValidator<AtualizarCargoRequest>
    {
        public AtualizarCargoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MaximumLength(50).WithMessage("Nome deve ter no máximo 50 caracteres.");

            RuleFor(x => x.Peso)
                .GreaterThan(0).WithMessage("Peso deve ser maior que zero.");
        }
    }
}
