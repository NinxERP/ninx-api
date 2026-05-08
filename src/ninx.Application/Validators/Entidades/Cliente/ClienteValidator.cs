using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class ClienteValidator : AbstractValidator<Cliente>
    {
        public ClienteValidator()
        {
            RuleFor(x => x.ClienteID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID do cliente deve ser maior ou igual a zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Telefone)
                .MinimumLength(10).WithMessage("Telefone deve ter no mínimo 10 caracteres.")
                .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Telefone));

            RuleFor(x => x.LimiteCredito)
                .GreaterThan(0).WithMessage("Limite de crédito deve ser maior que zero.")
                .When(x => x.LimiteCredito.HasValue);

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualização deve ser maior que a data de criação.")
                .When(x => x.AtualizadoEm.HasValue);
        }
    }
}
