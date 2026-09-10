using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class SolicitarRedefinicaoSenhaRequestValidator : AbstractValidator<SolicitarRedefinicaoSenhaRequest>
    {
        public SolicitarRedefinicaoSenhaRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail é obrigatório.")
                .EmailAddress().WithMessage("E-mail inválido.");
        }
    }
}
