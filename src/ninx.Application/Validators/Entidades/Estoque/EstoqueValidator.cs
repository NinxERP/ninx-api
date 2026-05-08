using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class EstoqueValidator : AbstractValidator<Estoque>
    {
        public EstoqueValidator()
        {
            RuleFor(x => x.EstoqueID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID do estoque deve ser maior ou igual a zero.");

            RuleFor(x => x.ProdutoID)
                .GreaterThan(0).WithMessage("O ID do produto deve ser maior que zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.Quantidade)
                .GreaterThanOrEqualTo(0).WithMessage("Quantidade deve ser maior ou igual a zero.");

            RuleFor(x => x.QuantidadeMinima)
                .GreaterThanOrEqualTo(0).WithMessage("Quantidade mínima deve ser maior ou igual a zero.");

            RuleFor(x => x.UltimaAtualizacao)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de última atualização não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.UltimaAtualizacao).WithMessage("Data de atualização deve ser maior ou igual à última atualização.")
                .When(x => x.AtualizadoEm.HasValue);
        }
    }
}
