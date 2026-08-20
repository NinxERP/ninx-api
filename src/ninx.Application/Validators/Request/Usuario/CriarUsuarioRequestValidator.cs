using FluentValidation;
using ninx.Communication;
using ninx.Domain.Interfaces;

namespace ninx.Application.Validators.Request
{
    public class CriarUsuarioRequestValidator : AbstractValidator<CriarUsuarioRequest>
    {
        public CriarUsuarioRequestValidator(ICargoRepository cargoRepository)
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("Nome � obrigat�rio.")
                .MinimumLength(3).WithMessage("Nome deve ter no m�nimo 3 caracteres.")
                .MaximumLength(150).WithMessage("Nome deve ter no m�ximo 150 caracteres.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("E-mail � obrigat�rio.")
                .EmailAddress().WithMessage("E-mail inv�lido.");

            RuleFor(x => x.Senha)
                .NotEmpty().WithMessage("Senha � obrigat�ria.")
                .MinimumLength(6).WithMessage("Senha deve ter no m�nimo 6 caracteres.")
                .MaximumLength(100).WithMessage("Senha deve ter no m�ximo 100 caracteres.");

            RuleFor(x => x.CargoID)
                .GreaterThan(0).WithMessage("O cargo deve ser informado.")
                .MustAsync(async (request, cargoId, _, cancellation) =>
                {
                    var cargo = await cargoRepository.GetByIdAsync(cargoId);
                    return cargo != null && cargo.Ativo && (cargo.ComercioID == null || cargo.ComercioID == request.ComercioId);
                }).WithMessage("O cargo informado é inválido para este comércio.");

            RuleFor(x => x.ComercioId)
                .GreaterThan(0).WithMessage("O ID do com�rcio deve ser maior que zero.");
        }
    }
}
