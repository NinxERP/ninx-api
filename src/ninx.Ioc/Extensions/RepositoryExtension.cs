using Microsoft.Extensions.DependencyInjection;
using ninx.Application.Services;
using ninx.Domain.Interfaces;
using ninx.Domain.Interfaces.Repositories;
using ninx.Infra.Repository;
using ninx.Infra.Repository.ClienteRepository;

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
            services.AddScoped<IEstoqueRepository, EstoqueRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IVendaRepository, VendaRepository>();
            services.AddScoped<IItemVendaRepository, ItemVendaRepository>();
            services.AddScoped<IMovimentacaoEstoqueRepository, MovimentacaoEstoqueRepository>();
            services.AddScoped<IPagamentoVendaRepository, PagamentoVendaRepository>();
            services.AddScoped<IAssinaturaEletronicaRepository, AssinaturaEletronicaRepository>();
            services.AddScoped<ICategoriaProdutoRepository, CategoriaProdutoRepository>();
            services.AddScoped<IAssinaturaPlanoRepository, AssinaturaPlanoRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IComercioService, ComercioService>();
            services.AddScoped<IPagamentoHistoricoAssinaturaPlanoRepository, PagamentoHistoricoAssinaturaPlanoRepository>();

            return services;
        }
    }
}