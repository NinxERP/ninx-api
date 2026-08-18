using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ResetarSenhaRequestValidator : AbstractValidator<ResetarSenhaRequest>
    {
        public ResetarSenhaRequestValidator()
        {
            RuleFor(x => x.NovaSenha)
                .NotEmpty().WithMessage("Nova senha é obrigatória.")
                .MinimumLength(6).WithMessage("Nova senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(100).WithMessage("Nova senha deve ter no máximo 100 caracteres.");
        }
    }
}
