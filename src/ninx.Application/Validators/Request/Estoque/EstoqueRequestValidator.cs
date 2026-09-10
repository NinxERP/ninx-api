using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class EstoqueRequestValidator : AbstractValidator<EstoqueRequest>
    {
        public EstoqueRequestValidator()
        {
            RuleFor(x => x.ProdutoID)
                .GreaterThan(0).WithMessage("O ID do produto deve ser maior que zero.");

            RuleFor(x => x.Quantidade)
                .GreaterThanOrEqualTo(0).WithMessage("Quantidade deve ser maior ou igual a zero.");

            RuleFor(x => x.QuantidadeMinima)
                .GreaterThanOrEqualTo(0).WithMessage("Quantidade mínima deve ser maior ou igual a zero.");
        }
    }
}
