using FluentValidation;
using ninx.Application.Validators;
using ninx.Communication;
using ninx.Domain.Enums;

namespace ninx.Application.Validators.Request
{
    public class PessoaAutorizadaRequestValidator : AbstractValidator<PessoaAutorizadaRequest>
    {
        public PessoaAutorizadaRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Cpf)
                .Must(Cpf.Valido).WithMessage("CPF inválido.")
                .When(x => !string.IsNullOrWhiteSpace(x.Cpf));

            RuleFor(x => x.Parentesco)
                .Must(p => Enum.IsDefined(typeof(ParentescoAutorizado), p)).WithMessage("Parentesco inválido.");

            RuleFor(x => x.LimiteCredito)
                .GreaterThan(0).WithMessage("Limite de crédito deve ser maior que zero.")
                .When(x => x.LimiteCredito.HasValue);
        }
    }
}
