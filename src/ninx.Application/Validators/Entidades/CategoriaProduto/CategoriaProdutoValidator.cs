using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class CategoriaProdutoValidator : AbstractValidator<CategoriaProduto>
    {
        public CategoriaProdutoValidator()
        {
            RuleFor(x => x.CategoriaID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID da categoria deve ser maior ou igual a zero.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");
        }
    }
}
