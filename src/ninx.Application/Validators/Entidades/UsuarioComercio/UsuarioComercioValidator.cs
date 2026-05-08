using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class UsuarioComercioValidator : AbstractValidator<UsuarioComercio>
    {
        public UsuarioComercioValidator()
        {
            RuleFor(x => x.UsuarioComercioID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID da relação usuário-comércio deve ser maior ou igual a zero.");

            RuleFor(x => x.UsuarioID)
                .GreaterThan(0).WithMessage("O ID do usuário deve ser maior que zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");
        }
    }
}
