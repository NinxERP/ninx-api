using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request
{
    public class CriarVendaRequestValidator : AbstractValidator<CriarVendaRequest>
    {
        public CriarVendaRequestValidator()
        {
            RuleFor(x => x.ClienteID)
                .GreaterThan(0).WithMessage("O ID do cliente deve ser maior que zero.")
                .When(x => x.ClienteID.HasValue && x.ClienteID.Value != 0);

            RuleFor(x => x.TipoVenda)
                .GreaterThan(0).WithMessage("Tipo de venda inválido.");

            RuleFor(x => x.ItensVenda)
                .NotEmpty().WithMessage("Venda deve ter no mínimo um item.")
                .Must(itens => itens.Count > 0).WithMessage("Lista de itens não pode estar vazia.");

            RuleFor(x => x.Pagamentos)
                .NotEmpty().WithMessage("Venda deve ter no mínimo um pagamento.")
                .Must(pagamentos => pagamentos.Count > 0).WithMessage("Lista de pagamentos não pode estar vazia.");

            RuleForEach(x => x.ItensVenda).SetValidator(new ItemVendaRequestValidator());
            RuleForEach(x => x.Pagamentos).SetValidator(new PagamentoVendaRequestValidator());
        }
    }
}
