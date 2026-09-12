using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class CriarCargoRequestValidator : AbstractValidator<CriarCargoRequest>
    {
        public CriarCargoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MaximumLength(50).WithMessage("Nome deve ter no máximo 50 caracteres.");

            RuleForEach(x => x.PermissaoIds)
                .GreaterThan(0).WithMessage("PermissaoID deve ser maior que zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.")
                .When(x => x.ComercioID.HasValue);
        }
    }
}
