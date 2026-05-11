using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ComercioRequestValidator : AbstractValidator<ComercioRequest>
    {
        public ComercioRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Endereco)
                .MaximumLength(250).WithMessage("Endereço deve ter no máximo 250 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Endereco));

            RuleFor(x => x.CNPJ)
                .Must(ValidarCNPJ).WithMessage("CNPJ inválido.")
                .When(x => !string.IsNullOrEmpty(x.CNPJ));
        }

        private bool ValidarCNPJ(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj))
                return true;

            cnpj = cnpj.Replace(".", "").Replace("/", "").Replace("-", "");

            if (cnpj.Length != 14)
                return false;

            if (!cnpj.All(char.IsDigit))
                return false;

            return true;
        }
    }
}
