using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class RelatorioProdutosVencendoRequestValidator : AbstractValidator<RelatorioProdutosVencendoRequest>
    {
        public RelatorioProdutosVencendoRequestValidator()
        {
            RuleFor(x => x.DiasLimite)
                .GreaterThan(0).WithMessage("Dias limite deve ser maior que zero.");
        }
    }
}
