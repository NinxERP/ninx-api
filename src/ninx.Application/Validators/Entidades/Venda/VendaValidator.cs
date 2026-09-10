using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class VendaValidator : AbstractValidator<Venda>
    {
        public VendaValidator()
        {
            RuleFor(x => x.VendaID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID da venda deve ser maior ou igual a zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.UsuarioID)
                .GreaterThan(0).WithMessage("O ID do usuário deve ser maior que zero.");

            RuleFor(x => x.ClienteID)
                .GreaterThan(0).WithMessage("O ID do cliente deve ser maior que zero.")
                .When(x => x.ClienteID.HasValue);

            RuleFor(x => x.Total)
                .GreaterThan(0).WithMessage("Total deve ser maior que zero.");

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de criação não pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualização deve ser maior que a data de criação.")
                .When(x => x.AtualizadoEm.HasValue);
        }
    }
}
