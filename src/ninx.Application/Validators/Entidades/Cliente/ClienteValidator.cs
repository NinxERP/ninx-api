using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class ClienteValidator : AbstractValidator<Cliente>
    {
        private static readonly HashSet<string> UFsValidas = new()
        {
            "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
            "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
            "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
        };

        public ClienteValidator()
        {
            RuleFor(x => x.ClienteID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID do cliente deve ser maior ou igual a zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Telefone)
                .MinimumLength(10).WithMessage("Telefone deve ter no mínimo 10 caracteres.")
                .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Telefone));

            RuleFor(x => x.Cpf)
                .NotEmpty().WithMessage("CPF é obrigatório.")
                .Must(ValidarCpf).WithMessage("CPF inválido.");

            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("E-mail inválido.")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.EnderecoLogradouro)
                .NotEmpty().WithMessage("Logradouro é obrigatório.")
                .MaximumLength(200).WithMessage("Logradouro deve ter no máximo 200 caracteres.");

            RuleFor(x => x.EnderecoNumero)
                .NotEmpty().WithMessage("Número é obrigatório.")
                .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres.");

            RuleFor(x => x.EnderecoComplemento)
                .MaximumLength(100).WithMessage("Complemento deve ter no máximo 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.EnderecoComplemento));

            RuleFor(x => x.EnderecoBairro)
                .NotEmpty().WithMessage("Bairro é obrigatório.")
                .MaximumLength(100).WithMessage("Bairro deve ter no máximo 100 caracteres.");

            RuleFor(x => x.EnderecoCidade)
                .NotEmpty().WithMessage("Cidade é obrigatória.")
                .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.");

            RuleFor(x => x.EnderecoUF)
                .NotEmpty().WithMessage("UF é obrigatória.")
                .Must(uf => UFsValidas.Contains(uf.ToUpperInvariant())).WithMessage("UF inválida.");

            RuleFor(x => x.EnderecoCEP)
                .NotEmpty().WithMessage("CEP é obrigatório.")
                .Must(ValidarCep).WithMessage("CEP inválido.");

            RuleFor(x => x.LimiteCredito)
                .GreaterThan(0).WithMessage("Limite de crédito deve ser maior que zero.")
                .When(x => x.LimiteCredito.HasValue);

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualização deve ser maior que a data de criação.")
                .When(x => x.AtualizadoEm.HasValue);
        }

        private static bool ValidarCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return false;

            var digitos = new string(cep.Where(char.IsDigit).ToArray());
            return digitos.Length == 8;
        }

        private static bool ValidarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                return false;

            cpf = new string(cpf.Where(char.IsDigit).ToArray());

            if (cpf.Length != 11)
                return false;

            if (new string(cpf[0], 11) == cpf)
                return false;

            var multiplicador1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            var multiplicador2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            var tempCpf = cpf.Substring(0, 9);
            var soma = 0;
            for (var i = 0; i < 9; i++)
                soma += (tempCpf[i] - '0') * multiplicador1[i];

            var resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            var digitoVerificador = resto.ToString();
            tempCpf += digitoVerificador;

            soma = 0;
            for (var i = 0; i < 10; i++)
                soma += (tempCpf[i] - '0') * multiplicador2[i];

            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;

            digitoVerificador += resto.ToString();

            return cpf.EndsWith(digitoVerificador);
        }
    }
}
