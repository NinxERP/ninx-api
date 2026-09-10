using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ninx.Api.Filters
{
    public class AuthorizeOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var declaringType = context.MethodInfo.DeclaringType;

            var hasAuthorize = context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                || (declaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false);

            var hasAllowAnonymous = context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
                || (declaringType?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() ?? false);

            if (!hasAuthorize || hasAllowAnonymous)
            {
                operation.Security = null;
                return;
            }

            operation.Responses ??= new OpenApiResponses();
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Não autenticado" });

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
                }
            };
        }
    }
}
