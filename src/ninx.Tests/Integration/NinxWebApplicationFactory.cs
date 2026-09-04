using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ninx.Api.Filters;
using ninx.Api.Middlewares;
using ninx.Data.Context;
using ninx.Ioc.Extensions;

namespace ninx.Tests.Integration
{
    /// <summary>
    /// Host de testes de integração para a API.
    /// <para>
    /// ninx.Api usa "top-level statements" em Program.cs, cuja classe Program gerada pelo
    /// compilador é <c>internal</c> — não há <c>InternalsVisibleTo</c> configurado no projeto
    /// ninx.Api apontando para ninx.Tests, e por instrução explícita deste trabalho não devemos
    /// alterar código de produção só para viabilizar testes (ver relatório final para essa nota
    /// de testabilidade).
    /// </para>
    /// <para>
    /// Em vez de <c>WebApplicationFactory&lt;ninx.Api.Program&gt;</c> — que também se mostrou frágil
    /// aqui por depender de heurísticas de "content root" baseadas na assembly do TEntryPoint,
    /// heurísticas essas pensadas para um TEntryPoint que é o próprio assembly da aplicação sob
    /// teste, não um marcador vivendo na assembly de testes — esta classe monta manualmente,
    /// com <see cref="Host"/> + <see cref="TestServer"/>, o mesmo pipeline montado em Program.cs a
    /// partir de tipos públicos: AddInfrastructure, filtros, middleware de exceção,
    /// autenticação/autorização e MapControllers — trocando apenas o SQL Server real por SQLite em memória.
    /// </para>
    /// </summary>
    public class NinxWebApplicationFactory : IDisposable
    {
        // VendaService, RedefinicaoSenhaService etc. usam transações explícitas via IUnitOfWork
        // (Database.BeginTransactionAsync), que o provider EF Core InMemory não suporta. Por isso os
        // testes de integração usam SQLite em memória (provider relacional "de verdade", com suporte
        // a transações) em vez de UseInMemoryDatabase. A conexão precisa ficar aberta durante toda a
        // vida da factory, senão o banco SQLite ":memory:" é descartado.
        private readonly SqliteConnection _connection;
        private readonly Lazy<IHost> _host;

        public NinxWebApplicationFactory()
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _host = new Lazy<IHost>(ConstruirHost);
        }

        public IServiceProvider Services => _host.Value.Services;

        public HttpClient CreateClient()
        {
            var testServer = _host.Value.Services.GetRequiredService<IServer>() as TestServer
                ?? throw new InvalidOperationException("TestServer não encontrado.");
            return testServer.CreateClient();
        }

        private IHost ConstruirHost()
        {
            var builder = new HostBuilder()
                .ConfigureWebHost(webBuilder =>
                {
                    webBuilder.UseTestServer();

                    webBuilder.ConfigureAppConfiguration(config =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Jwt:Issuer"] = "NinxSGC",
                            ["Jwt:Audience"] = "NinxSGCClient",
                            ["Jwt:Secret"] = "REMOVIDO-segredo-substituido-no-historico-0000",
                            ["Jwt:ExpiresInMinutes"] = "1200",
                            ["Brevo:ApiKey"] = "fake-key-testes",
                            ["Brevo:SenderEmail"] = "testes@ninx.local",
                            ["Brevo:SenderName"] = "Ninx Testes",
                            ["ConnectionStrings:DefaultConnection"] = "Server=(local);Database=NaoUsado;Trusted_Connection=True;"
                        });
                    });

                    webBuilder.ConfigureServices((context, services) =>
                    {
                        services.AddRouting();
                        services.AddControllers(options =>
                        {
                            options.Filters.Add<ValidationActionFilter>();
                        }).AddApplicationPart(typeof(ninx.Api.Controllers.LoginController).Assembly);

                        // Não usamos ninx.Ioc.DependencyInjection.AddInfrastructure aqui porque ela chama
                        // AddDatabase (EF Core + SqlServer). Registrar SqlServer e depois tentar substituir
                        // pelo provider InMemory no mesmo IServiceCollection faz o EF Core reclamar de
                        // múltiplos database providers registrados (os serviços internos de cada provider
                        // ficam ambos presentes). Por isso chamamos as demais extensões públicas de
                        // ninx.Ioc individualmente e registramos apenas o provider InMemory.
                        services.AddJwtAuthentication(context.Configuration);
                        services.AddMapster();
                        services.AddRepositories();
                        services.AddServices();
                        services.AddValidators();
                        services.AddEmail(context.Configuration);

                        services.AddDbContext<NinxDB>(options => options
                            .UseSqlite(_connection)
                            .ReplaceService<Microsoft.EntityFrameworkCore.Infrastructure.IModelCustomizer, SqliteColumnTypeStrippingModelCustomizer>());
                    });

                    webBuilder.Configure(app =>
                    {
                        app.UseMiddleware<ExceptionMiddleware>();
                        app.UseRouting();
                        app.UseAuthentication();
                        app.UseAuthorization();
                        app.UseEndpoints(endpoints => endpoints.MapControllers());
                    });
                });

            var host = builder.Build();
            host.Start();

            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<NinxDB>();
                db.Database.EnsureCreated();
            }

            return host;
        }

        public void Dispose()
        {
            if (_host.IsValueCreated)
            {
                _host.Value.Dispose();
            }
            _connection.Dispose();
        }
    }
}
