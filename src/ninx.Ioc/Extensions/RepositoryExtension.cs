using Microsoft.Extensions.DependencyInjection;
using ninx.Domain.Interfaces.Repositories;
using ninx.Infra.Repository;

namespace ninx.Ioc.Extensions
{
    public static class RepositoryExtension
    {
        public static IServiceCollection AddRepositories(
            this IServiceCollection services)
        {
            services.AddScoped<IProdutoRepository, ProdutoRepository>();
            services.AddScoped<IComercioRepository, ComercioRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IUsuarioComercioRepository, UsuarioComercioRepository>();

            return services;
        }
    }
}