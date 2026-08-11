using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using ninx.Domain.Exceptions;

namespace ninx.Api.Filters
{
    public class ValidationActionFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationActionFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument is null)
                    continue;

                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                if (_serviceProvider.GetService(validatorType) is not IValidator validator)
                    continue;

                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext);

                if (!result.IsValid)
                {
                    var mensagem = string.Join(" ", result.Errors.Select(e => e.ErrorMessage));
                    throw new BadRequestException(mensagem);
                }
            }

            await next();
        }
    }
}
