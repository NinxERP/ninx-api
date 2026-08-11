using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class CategoriaProdutoRequestValidator : AbstractValidator<CategoriaProdutoRequest>
    {
        public CategoriaProdutoRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");
        }
    }
}
