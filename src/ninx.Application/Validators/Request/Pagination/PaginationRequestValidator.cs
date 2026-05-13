using FluentValidation;
using ninx.Communication;

namespace ninx.Application.Validators.Request.Pagination
{
    public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
    {
        public PaginationRequestValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("PageNumber deve ser maior que 0")
                .When(x => x.PageNumber > 0);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize deve ser maior que 0")
                .LessThanOrEqualTo(100)
                .WithMessage("PageSize não pode ser maior que 100");
        }
    }
}
