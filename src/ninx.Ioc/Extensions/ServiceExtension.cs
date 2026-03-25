using Microsoft.Extensions.DependencyInjection;
using ninx.Application.Interfaces.Services;
using ninx.Application.Services;
using ninx.Domain.Interfaces.Services;

namespace ninx.Ioc.Extensions
{
    public static class ServiceExtension
    {
        public static IServiceCollection AddServices(
            this IServiceCollection services)
        {
            services.AddScoped<IProdutoService, ProdutoService>();
            services.AddScoped<IComercioService, ComercioService>();
            services.AddScoped<ITrocarComercioService, TrocarComercioService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IUsuarioComercioService, UsuarioComercioService>();
            return services;
        }
    }
}