using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class ProdutoValidator : AbstractValidator<Produto>
    {
        public ProdutoValidator()
        {
            RuleFor(x => x.ProdutoID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID do produto deve ser maior ou igual a zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

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

            RuleFor(x => x.Validade)
                .GreaterThan(DateTime.Now).WithMessage("Data de validade deve ser futura.")
                .When(x => x.Validade.HasValue);

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualização deve ser maior que a data de criação.")
                .When(x => x.AtualizadoEm.HasValue);
        }
    }
}
