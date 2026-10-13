using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class ConfirmarAssinaturaEletronicaRequestValidator : AbstractValidator<ConfirmarAssinaturaEletronicaRequest>
    {
        private const int MaxPontos = 20_000;

        public ConfirmarAssinaturaEletronicaRequestValidator()
        {
            RuleFor(x => x.Tracos)
                .NotNull().WithMessage("A assinatura é obrigatória.")
                .Must(t => t != null && t.Any(traco => traco.Pontos?.Count >= 2)).WithMessage("A assinatura é obrigatória.")
                .Must(t => t == null || t.Sum(traco => traco.Pontos?.Count ?? 0) <= MaxPontos).WithMessage("A assinatura tem pontos demais.");

            RuleForEach(x => x.Tracos).ChildRules(traco =>
            {
                traco.RuleFor(t => t.Pagina).GreaterThanOrEqualTo(1).WithMessage("Página inválida.");
                traco.RuleFor(t => t.Espessura).InclusiveBetween(0.5, 10).WithMessage("Espessura do traço inválida.");
                traco.RuleFor(t => t.Pontos).NotNull().WithMessage("Traço sem pontos.");
                traco.RuleForEach(t => t.Pontos).ChildRules(ponto =>
                {
                    ponto.RuleFor(p => p.X).InclusiveBetween(0, 1).WithMessage("Ponto fora da página.");
                    ponto.RuleFor(p => p.Y).InclusiveBetween(0, 1).WithMessage("Ponto fora da página.");
                });
            });

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude inválida.")
                .When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude inválida.")
                .When(x => x.Longitude.HasValue);
        }
    }
}
