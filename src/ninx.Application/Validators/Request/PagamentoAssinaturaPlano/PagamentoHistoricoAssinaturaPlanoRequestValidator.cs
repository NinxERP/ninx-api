using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class PagamentoHistoricoAssinaturaPlanoRequestValidator : AbstractValidator<PagamentoHistoricoAssinaturaPlanoRequest>
    {
        public PagamentoHistoricoAssinaturaPlanoRequestValidator()
        {
            RuleFor(x => x.ComercioId)
                .GreaterThan(0).WithMessage("O ID do comércio deve ser maior que zero.");

            RuleFor(x => x.Valor)
                .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
        }
    }
}
