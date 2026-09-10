using FluentValidation;
using ninx.Communication;
using ninx.Domain.Interfaces;

namespace ninx.Application.Validators.Request
{
    public class AtualizarUsuarioComercioRequestValidator : AbstractValidator<AtualizarUsuarioComercioRequest>
    {
        public AtualizarUsuarioComercioRequestValidator(ICargoRepository cargoRepository)
        {
            RuleFor(x => x.UsuarioID)
                .GreaterThan(0).WithMessage("O ID do usuário deve ser maior que zero.");

            RuleFor(x => x.ComercioID)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.CargoID)
                .MustAsync(async (request, cargoId, _, cancellation) =>
                {
                    var cargo = await cargoRepository.GetByIdAsync(cargoId);
                    return cargo != null && cargo.Ativo && (cargo.ComercioID == null || cargo.ComercioID == request.ComercioID);
                }).WithMessage("O cargo informado é inválido para este comércio.")
                .When(x => x.CargoID != 0);
        }
    }
}
