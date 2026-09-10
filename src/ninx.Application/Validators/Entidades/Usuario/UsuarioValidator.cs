using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class UsuarioValidator : AbstractValidator<Usuario>
    {
        public UsuarioValidator()
        {
            RuleFor(x => x.UsuarioID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID do usuário deve ser maior ou igual a zero.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório.")
                .EmailAddress().WithMessage("E-mail inválido.");

            RuleFor(x => x.SenhaHash)
                .NotEmpty().WithMessage("Senha hash é obrigatória.");

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualização deve ser maior que a data de criação.")
                .When(x => x.AtualizadoEm.HasValue);
        }
    }
}
