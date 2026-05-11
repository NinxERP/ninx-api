using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ClienteRequestValidator : AbstractValidator<ClienteRequest>
    {
        public ClienteRequestValidator()
        {
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
        }
    }
}
