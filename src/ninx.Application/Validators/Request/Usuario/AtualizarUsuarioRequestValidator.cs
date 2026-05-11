using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class AtualizarUsuarioRequestValidator : AbstractValidator<AtualizarUsuarioRequest>
    {
        public AtualizarUsuarioRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório.")
                .EmailAddress().WithMessage("E-mail inválido.");
        }
    }
}
