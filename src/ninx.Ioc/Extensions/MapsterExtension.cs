using Mapster;
using Microsoft.Extensions.DependencyInjection;
using ninx.Application.Services;
using System.Reflection;

namespace ninx.Ioc.Extensions
{
    public static class MapsterExtension
    {
        public static IServiceCollection AddMapster(
            this IServiceCollection services)
        {
            var config = TypeAdapterConfig.GlobalSettings;
            config.Scan(Assembly.GetAssembly(typeof(ProdutoService))!);
            services.AddSingleton(config);

            return services;
        }
    }
}