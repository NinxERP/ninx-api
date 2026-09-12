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

            RuleForEach(x => x.PermissaoIds)
                .GreaterThan(0).WithMessage("PermissaoID deve ser maior que zero.");
        }
    }
}
