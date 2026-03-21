using Microsoft.Extensions.DependencyInjection;
using ninx.Application.Interfaces.Services.Comercio;
using ninx.Application.Interfaces.Services.Login;
using ninx.Application.Services;
using ninx.Application.Services.JwtToken;
using ninx.Application.Services.Login;
using ninx.Domain.Interfaces.Services;
using ninx.Domain.Interfaces.Services.JwtToken;

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