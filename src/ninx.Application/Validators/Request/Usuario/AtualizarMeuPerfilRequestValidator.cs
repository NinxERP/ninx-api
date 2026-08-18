using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class AtualizarMeuPerfilRequestValidator : AbstractValidator<AtualizarMeuPerfilRequest>
    {
        public AtualizarMeuPerfilRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório.")
                .EmailAddress().WithMessage("E-mail inválido.");

            RuleFor(x => x.Senha)
                .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres.")
                .When(x => x.Senha != null);
        }
    }
}
