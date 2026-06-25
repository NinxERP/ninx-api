using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ConfirmarDocumentosVendaRequestValidator : AbstractValidator<ConfirmarAssinaturaEletronicaRequest>
    {
        public ConfirmarDocumentosVendaRequestValidator()
        {
            RuleFor(x => x.ImagemBase64)
                .NotEmpty().WithMessage("Imagem em base64 é obrigatória.")
                .Must(ValidarBase64).WithMessage("Imagem inválida ou não está em formato base64.");

            RuleFor(x => x.Latitude)
                .GreaterThanOrEqualTo(-90).WithMessage("Latitude inválida.")
                .LessThanOrEqualTo(90).WithMessage("Latitude inválida.")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .GreaterThanOrEqualTo(-180).WithMessage("Longitude inválida.")
                .LessThanOrEqualTo(180).WithMessage("Longitude inválida.")
                .When(x => x.Longitude.HasValue);
        }

        private bool ValidarBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return false;

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
