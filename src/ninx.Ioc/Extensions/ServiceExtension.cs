using Microsoft.Extensions.DependencyInjection;
using ninx.Application.Services;
using ninx.Application.Services.TrocarComercio;
using ninx.Domain.Interfaces;
using ninx.Infra;

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
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IUsuarioComercioService, UsuarioComercioService>();
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<IVendaService, VendaService>();
            services.AddScoped<IAssinaturaEletronicaService, AssinaturaEletronicaService>();
            services.AddScoped<IEstoqueService, EstoqueService>();
            services.AddScoped<ICategoriaProdutoService, CategoriaProdutoService>();
            services.AddScoped<ITokenProvider, TokenProvider>();
            return services;
        }
    }
}