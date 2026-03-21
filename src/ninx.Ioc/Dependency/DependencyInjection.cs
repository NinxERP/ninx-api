using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ninx.Ioc.Extensions;

namespace ninx.Ioc
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDatabase(configuration);
            services.AddJwtAuthentication(configuration);
            services.AddMapster();
            services.AddRepositories();
            services.AddServices();

            return services;
        }
    }
}