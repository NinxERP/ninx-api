using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class MovimentacaoEstoqueValidator : AbstractValidator<MovimentacaoEstoque>
    {
        public MovimentacaoEstoqueValidator()
        {
            RuleFor(x => x.MovimentacaoID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID da movimentação deve ser maior ou igual a zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.ProdutoID)
                .GreaterThan(0).WithMessage("O ID do produto deve ser maior que zero.");

            RuleFor(x => x.UsuarioID)
                .GreaterThan(0).WithMessage("O ID do usuário deve ser maior que zero.");

            RuleFor(x => x.Quantidade)
                .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");

            RuleFor(x => x.VendaID)
                .GreaterThan(0).WithMessage("O ID da venda deve ser maior que zero.")
                .When(x => x.VendaID.HasValue);

            RuleFor(x => x.Observacao)
                .MaximumLength(500).WithMessage("Observação deve ter no máximo 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Observacao));

            RuleFor(x => x.DataHora)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data e hora não podem ser futuros.");
        }
    }
}
