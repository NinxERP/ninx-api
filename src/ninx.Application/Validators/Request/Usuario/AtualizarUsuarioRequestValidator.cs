using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class AtualizarUsuarioRequestValidator : AbstractValidator<AtualizarUsuarioRequest>
    {
        public AtualizarUsuarioRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome � obrigat�rio.")
                .MinimumLength(3).WithMessage("Nome deve ter no m�nimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no m�ximo 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail � obrigat�rio.")
                .EmailAddress().WithMessage("E-mail inv�lido.");

            RuleFor(x => x.CargoID)
                .GreaterThan(0).WithMessage("CargoID inv�lido.")
                .When(x => x.CargoID.HasValue);
        }
    }
}
