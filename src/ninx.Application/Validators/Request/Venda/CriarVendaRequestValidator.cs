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
                .When(x => x.ClienteID.HasValue);

            RuleFor(x => x.Observacoes)
                .MaximumLength(500).WithMessage("Observa��es deve ter no m�ximo 500 caracteres.")
                .When(x => !string.IsNullOrEmpty(x.Observacoes));

            RuleFor(x => x.TipoVenda)
                .GreaterThan(0).WithMessage("Tipo de venda inv�lido.");

            RuleFor(x => x.ItensVenda)
                .NotEmpty().WithMessage("Venda deve ter no m�nimo um item.")
                .Must(itens => itens.Count > 0).WithMessage("Lista de itens n�o pode estar vazia.");

            RuleFor(x => x.Pagamentos)
                .NotEmpty().WithMessage("Venda deve ter no m�nimo um pagamento.")
                .Must(pagamentos => pagamentos.Count > 0).WithMessage("Lista de pagamentos n�o pode estar vazia.");

            RuleForEach(x => x.ItensVenda).SetValidator(new ItemVendaRequestValidator());
            RuleForEach(x => x.Pagamentos).SetValidator(new PagamentoVendaRequestValidator());
        }
    }
}
