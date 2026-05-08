using FluentValidation;
using ninx.Communication.Request;

namespace ninx.Application.Validators.Request
{
    public class CriarUsuarioRequestValidator : AbstractValidator<CriarUsuarioRequest>
    {
        public CriarUsuarioRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório.")
                .EmailAddress().WithMessage("E-mail inválido.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Senha é obrigatória.")
                .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres.")
                .MaximumLength(100).WithMessage("Senha deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Permissao)
                .GreaterThan(0).WithMessage("Permissão deve ser maior que zero.")
                .LessThanOrEqualTo(3).WithMessage("Permissão inválida.");

            RuleFor(x => x.ComercioId)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");
        }
    }
}
