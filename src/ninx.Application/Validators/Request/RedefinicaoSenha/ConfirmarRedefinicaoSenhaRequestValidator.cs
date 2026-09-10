using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ConfirmarRedefinicaoSenhaRequestValidator : AbstractValidator<ConfirmarRedefinicaoSenhaRequest>
    {
        public ConfirmarRedefinicaoSenhaRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório.")
                .EmailAddress().WithMessage("E-mail inválido.");

            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("Código é obrigatório.")
                .Length(6).WithMessage("Código deve ter 6 dígitos.")
                .Matches("^[0-9]{6}$").WithMessage("Código deve conter apenas números.");

            RuleFor(x => x.NovaSenha)
                .NotEmpty().WithMessage("Nova senha é obrigatória.")
                .MinimumLength(6).WithMessage("Nova senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(100).WithMessage("Nova senha deve ter no máximo 100 caracteres.");
        }
    }
}
