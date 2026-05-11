using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ninx.Application.Validators.Entidades;
using ninx.Application.Validators.Request;
using ninx.Communication;
using ninx.Communication.Venda;
using ninx.Domain.Entities;

namespace ninx.Ioc.Extensions
{
    public static class ValidatorExtension
    {
        public static IServiceCollection AddValidators(
            this IServiceCollection services)
        {
            services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
            services.AddScoped<IValidator<CriarUsuarioRequest>, CriarUsuarioRequestValidator>();
            services.AddScoped<IValidator<AtualizarUsuarioRequest>, AtualizarUsuarioRequestValidator>();
            services.AddScoped<IValidator<ClienteRequest>, ClienteRequestValidator>();
            services.AddScoped<IValidator<ComercioRequest>, ComercioRequestValidator>();
            services.AddScoped<IValidator<CriarProdutoRequest>, CriarProdutoRequestValidator>();
            services.AddScoped<IValidator<AtualizarProdutoRequest>, AtualizarProdutoRequestValidator>();
            services.AddScoped<IValidator<CriarVendaRequest>, CriarVendaRequestValidator>();
            services.AddScoped<IValidator<PagamentoVendaRequest>, PagamentoVendaRequestValidator>();
            services.AddScoped<IValidator<ReceberPagamentoFiadoRequest>, ReceberPagamentoFiadoRequestValidator>();
            services.AddScoped<IValidator<ItemVendaRequest>, ItemVendaRequestValidator>();
            services.AddScoped<IValidator<CriarUsuarioComercioRequest>, CriarUsuarioComercioRequestValidator>();
            services.AddScoped<IValidator<AtualizarUsuarioComercioRequest>, AtualizarUsuarioComercioRequestValidator>();
            services.AddScoped<IValidator<ConfirmarAssinaturaEletronicaRequest>, ConfirmarAssinaturaEletronicaRequestValidator>();
            services.AddScoped<IValidator<FiltroRequest>, FiltroRequestValidator>();

            services.AddScoped<IValidator<Usuario>, UsuarioValidator>();
            services.AddScoped<IValidator<Comercio>, ComercioValidator>();
            services.AddScoped<IValidator<Cliente>, ClienteValidator>();
            services.AddScoped<IValidator<Produto>, ProdutoValidator>();
            services.AddScoped<IValidator<Venda>, VendaValidator>();
            services.AddScoped<IValidator<UsuarioComercio>, UsuarioComercioValidator>();
            services.AddScoped<IValidator<ItemVenda>, ItemVendaValidator>();
            services.AddScoped<IValidator<AssinaturaEletronica>, AssinaturaEletronicaValidator>();
            services.AddScoped<IValidator<Estoque>, EstoqueValidator>();
            services.AddScoped<IValidator<CategoriaProduto>, CategoriaProdutoValidator>();
            services.AddScoped<IValidator<PagamentoVenda>, PagamentoVendaValidator>();
            services.AddScoped<IValidator<MovimentacaoEstoque>, MovimentacaoEstoqueValidator>();
            services.AddScoped<IValidator<SessaoWhatsapp>, SessaoWhatsappValidator>();
            services.AddScoped<IValidator<AssinaturaPlano>, AssinaturaPlanoValidator>();

            return services;
        }
    }
}
