using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class ItemVendaValidator : AbstractValidator<ItemVenda>
    {
        public ItemVendaValidator()
        {
            RuleFor(x => x.ItemVendaID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID do item de venda deve ser maior ou igual a zero.");

            RuleFor(x => x.VendaID)
                .GreaterThan(0).WithMessage("O ID da venda deve ser maior que zero.");

            RuleFor(x => x.ProdutoID)
                .GreaterThan(0).WithMessage("O ID do produto deve ser maior que zero.");

            RuleFor(x => x.ProdutoNome)
                .NotEmpty().WithMessage("Nome do produto é obrigatório.")
                .MinimumLength(3).WithMessage("Nome do produto deve ter no mínimo 3 caracteres.");

            RuleFor(x => x.Quantidade)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");

            RuleFor(x => x.PrecoUnitario)
                .GreaterThan(0).WithMessage("Preço unitário deve ser maior que zero.");

            RuleFor(x => x.Subtotal)
                .GreaterThan(0).WithMessage("Subtotal deve ser maior que zero.");
        }
    }
}
