using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ComercioRequestValidator : AbstractValidator<ComercioRequest>
    {
        private static readonly HashSet<string> UFsValidas = new()
        {
            "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
            "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
            "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
        };

        public ComercioRequestValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome é obrigatório.")
                .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

            RuleFor(x => x.Endereco)
                .MaximumLength(250).WithMessage("Endereço deve ter no máximo 250 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Endereco));

            RuleFor(x => x.EnderecoLogradouro)
                .MaximumLength(200).WithMessage("Logradouro deve ter no máximo 200 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.EnderecoLogradouro));

            RuleFor(x => x.EnderecoNumero)
                .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.EnderecoNumero));

            RuleFor(x => x.EnderecoComplemento)
                .MaximumLength(100).WithMessage("Complemento deve ter no máximo 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.EnderecoComplemento));

            RuleFor(x => x.EnderecoBairro)
                .MaximumLength(100).WithMessage("Bairro deve ter no máximo 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.EnderecoBairro));

            RuleFor(x => x.EnderecoCidade)
                .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.EnderecoCidade));

            RuleFor(x => x.EnderecoUF)
                .Must(uf => UFsValidas.Contains(uf!.ToUpperInvariant())).WithMessage("UF inválida.")
                .When(x => !string.IsNullOrEmpty(x.EnderecoUF));

            RuleFor(x => x.EnderecoCEP)
                .Must(ValidarCep).WithMessage("CEP inválido.")
                .When(x => !string.IsNullOrEmpty(x.EnderecoCEP));

            RuleFor(x => x.CNPJ)
                .Must(ValidarCNPJ).WithMessage("CNPJ inválido.")
                .When(x => !string.IsNullOrEmpty(x.CNPJ));

            RuleFor(x => x.AssinaturaResponsavelBase64)
                .Must(ValidarBase64).WithMessage("Imagem de assinatura inválida ou não está em formato base64.")
                .When(x => !string.IsNullOrEmpty(x.AssinaturaResponsavelBase64));

            RuleFor(x => x.LimiteCreditoPadrao)
                .GreaterThan(0).WithMessage("Limite de crédito padrão deve ser maior que zero.")
                .When(x => x.LimiteCreditoPadrao.HasValue);
        }

        private static bool ValidarCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep))
                return false;

            var digitos = new string(cep.Where(char.IsDigit).ToArray());
            return digitos.Length == 8;
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

        private bool ValidarBase64(string? base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return true;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
