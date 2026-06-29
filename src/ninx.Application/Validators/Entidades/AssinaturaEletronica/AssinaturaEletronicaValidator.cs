using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class AssinaturaEletronicaValidator : AbstractValidator<AssinaturaEletronica>
    {
        public AssinaturaEletronicaValidator()
        {
            RuleFor(x => x.AssinaturaID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID da assinatura deve ser maior ou igual a zero.");

            RuleFor(x => x.VendaID)
                .GreaterThan(0).WithMessage("O ID da venda deve ser maior que zero.");

            RuleFor(x => x.DocumentoGuid)
                .NotEmpty().WithMessage("Guid do documento é obrigatório.");

            RuleFor(x => x.ImagemAssinatura)
                .Must(ValidarBase64).WithMessage("Imagem inválida ou não está em formato base64.")
                .When(x => !string.IsNullOrEmpty(x.ImagemAssinatura));

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualização deve ser maior que a data de criação.");

            RuleFor(x => x.DataAssinatura)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de assinatura não pode ser futura.")
                .When(x => x.DataAssinatura.HasValue);
        }

        private bool ValidarBase64(string base64String)
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
