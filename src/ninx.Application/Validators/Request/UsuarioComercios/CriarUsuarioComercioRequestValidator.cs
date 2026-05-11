using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class CriarUsuarioComercioRequestValidator : AbstractValidator<CriarUsuarioComercioRequest>
    {
        public CriarUsuarioComercioRequestValidator()
        {
            RuleFor(x => x.UsuarioID)
                .GreaterThan(0).WithMessage("O ID do usuário deve ser maior que zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");
        }
    }
}
