using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ninx.Domain.Interfaces;
using ninx.Infra;

namespace ninx.Ioc.Extensions
{
    public static class EmailExtension
    {
        public static IServiceCollection AddEmail(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddHttpClient<IEmailService, BrevoEmailService>(client =>
            {
                client.BaseAddress = new Uri("https://api.brevo.com/");
            });

            return services;
        }
    }
}
