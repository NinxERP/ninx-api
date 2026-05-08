using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class AssinaturaPlanoValidator : AbstractValidator<AssinaturaPlano>
    {
        public AssinaturaPlanoValidator()
        {
            RuleFor(x => x.AssinaturaID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID da assinatura deve ser maior ou igual a zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.DataInicio)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de início não pode ser futura.");

            RuleFor(x => x.DataFim)
                .GreaterThan(x => x.DataInicio).WithMessage("Data de término deve ser maior que data de início.");

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualização deve ser maior que a data de criação.")
                .When(x => x.AtualizadoEm.HasValue);
        }
    }
}
