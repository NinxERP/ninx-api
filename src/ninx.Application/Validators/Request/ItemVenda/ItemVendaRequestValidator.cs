using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ItemVendaRequestValidator : AbstractValidator<ItemVendaRequest>
    {
        public ItemVendaRequestValidator()
        {
            RuleFor(x => x.ProdutoID)
                .GreaterThan(0).WithMessage("O ID do produto deve ser maior que zero.");

            RuleFor(x => x.Quantidade)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
        }
    }
}
