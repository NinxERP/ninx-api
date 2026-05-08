using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class ComercioValidator : AbstractValidator<Comercio>
    {
        public ComercioValidator()
        {
            RuleFor(x => x.ComercioID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID do comércio deve ser maior ou igual a zero.");

            RuleFor(x => x.NomeComercio)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Endereco)
                .MaximumLength(250).WithMessage("Endereço deve ter no máximo 250 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Endereco));

            RuleFor(x => x.CNPJ)
                .Must(ValidarCNPJ).WithMessage("CNPJ inválido.")
                .When(x => !string.IsNullOrEmpty(x.CNPJ));

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualização deve ser maior que a data de criação.")
                .When(x => x.AtualizadoEm.HasValue);
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
