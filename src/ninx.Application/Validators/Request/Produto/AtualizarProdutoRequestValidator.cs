using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class AtualizarProdutoRequestValidator : AbstractValidator<AtualizarProdutoRequest>
    {
        public AtualizarProdutoRequestValidator()
        {
            RuleFor(x => x.CategoriaID)
                .GreaterThan(0).WithMessage("O ID da categoria deve ser maior que zero.")
                .When(x => x.CategoriaID.HasValue);

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.CodigoBarras)
                .MaximumLength(50).WithMessage("Código de barras deve ter no máximo 50 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.CodigoBarras));

            RuleFor(x => x.PrecoVenda)
                .GreaterThan(0).WithMessage("Preço de venda deve ser maior que zero.");

            RuleFor(x => x.PrecoCusto)
                .GreaterThan(0).WithMessage("Preço de custo deve ser maior que zero.")
                .When(x => x.PrecoCusto.HasValue);

            RuleFor(x => x.UnidadeMedida)
                .NotEmpty().WithMessage("Unidade de medida é obrigatória.")
                .MaximumLength(10).WithMessage("Unidade de medida deve ter no máximo 10 caracteres.");

            RuleFor(x => x.Validade)
                .GreaterThan(DateTime.Now).WithMessage("Data de validade deve ser futura.")
                .When(x => x.Validade.HasValue);
        }
    }
}
