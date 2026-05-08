using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class SessaoWhatsappValidator : AbstractValidator<SessaoWhatsapp>
    {
        public SessaoWhatsappValidator()
        {
            RuleFor(x => x.SessaoID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID da sessão deve ser maior ou igual a zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.NumeroCelular)
                .NotEmpty().WithMessage("Número de celular é obrigatório.")
                .MinimumLength(10).WithMessage("Número de celular deve ter no mínimo 10 caracteres.")
                .MaximumLength(20).WithMessage("Número de celular deve ter no máximo 20 caracteres.");

            RuleFor(x => x.DadosTemporarios)
                .MaximumLength(1000).WithMessage("Dados temporários deve ter no máximo 1000 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.DadosTemporarios));

            RuleFor(x => x.UltimaInteracao)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data da última interação não pode ser futura.");
        }
    }
}
