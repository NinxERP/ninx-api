using FluentValidation;
using ninx.Domain.Entities;

namespace ninx.Application.Validators.Entidades
{
    public class PagamentoVendaValidator : AbstractValidator<PagamentoVenda>
    {
        public PagamentoVendaValidator()
        {
            RuleFor(x => x.PagamentoID)
                .GreaterThanOrEqualTo(0).WithMessage("O ID do pagamento deve ser maior ou igual a zero.");

            RuleFor(x => x.VendaID)
                .GreaterThan(0).WithMessage("O ID da venda deve ser maior que zero.");

            RuleFor(x => x.UsuarioID)
                .GreaterThan(0).WithMessage("O ID do usu�rio deve ser maior que zero.");

            RuleFor(x => x.PagamentoVinculoID)
                .GreaterThan(0).WithMessage("O ID do pagamento v�nculo deve ser maior que zero.");

            RuleFor(x => x.Valor)
                .GreaterThanOrEqualTo(0).WithMessage("Valor não pode ser negativo.");

            RuleFor(x => x.CriadoEm)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de cria��o n�o pode ser futura.");

            RuleFor(x => x.AtualizadoEm)
                .GreaterThanOrEqualTo(x => x.CriadoEm).WithMessage("Data de atualiza��o deve ser maior que a data de cria��o.");
        }
    }
}
