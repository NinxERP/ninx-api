using FluentValidation;
using ninx.Communication.Request;

namespace ninx.Application.Validators.Request
{
    public class AtualizarUsuarioComercioRequestValidator : AbstractValidator<AtualizarUsuarioComercioRequest>
    {
        public AtualizarUsuarioComercioRequestValidator()
        {
            RuleFor(x => x.UsuarioID)
                .GreaterThan(0).WithMessage("O ID do usuário deve ser maior que zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.Permissao)
                .GreaterThan(0).WithMessage("Permissão deve ser maior que zero.")
                .LessThanOrEqualTo(3).WithMessage("Permissão inválida.");
        }
    }
}
