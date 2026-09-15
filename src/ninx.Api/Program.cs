using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using ninx.Api.Filters;
using ninx.Api.Middlewares;
using ninx.Ioc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

var envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFile))
{
    var envToConfigKey = new Dictionary<string, string>
    {
        ["DB_CONNECTION_STRING"] = "ConnectionStrings:DefaultConnection",
        ["JWT_SECRET"] = "Jwt:Secret",
        ["JWT_ISSUER"] = "Jwt:Issuer",
        ["JWT_AUDIENCE"] = "Jwt:Audience",
        ["JWT_EXPIRES_IN_MINUTES"] = "Jwt:ExpiresInMinutes",
        ["BREVO_API_KEY"] = "Brevo:ApiKey",
        ["BREVO_SENDER_EMAIL"] = "Brevo:SenderEmail",
        ["BREVO_SENDER_NAME"] = "Brevo:SenderName",
    };

    var envConfig = File.ReadAllLines(envFile)
        .Select(line => line.Split('=', 2))
        .Where(kv => kv.Length == 2 && envToConfigKey.ContainsKey(kv[0].Trim()) && kv[1].Trim().Length > 0)
        .ToDictionary(kv => envToConfigKey[kv[0].Trim()], kv => (string?)kv[1].Trim());

    builder.Configuration.AddInMemoryCollection(envConfig);
}

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
});
builder.Services.AddInfrastructure(builder.Configuration);

// A API só é alcançável pelo ingress do Azure Container Apps, cujos endereços internos
// não são fixos. As listas são limpas para confiar no proxy imediatamente anterior, e
// ForwardLimit = 1 faz usar só o último salto de X-Forwarded-For — o valor escrito pelo
// próprio ingress, que o cliente não consegue forjar. Sem isso, o IP registrado como
// evidência da assinatura era o que o cliente quisesse declarar no cabeçalho, e os
// limites de taxa por IP enxergavam o endereço do ingress em vez do cliente real.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.ForwardLimit = 1;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("RedefinicaoSenhaSolicitar", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientIp(context),
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 3,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0
        }));

    options.AddPolicy("RedefinicaoSenhaConfirmar", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientIp(context),
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0
        }));

    // Endpoints públicos da assinatura. Os limites são folgados porque vários clientes
    // podem assinar a partir do Wi-Fi da própria loja, compartilhando o mesmo IP.
    options.AddPolicy("AssinaturaPublicaLeitura", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientIp(context),
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 60,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    options.AddPolicy("AssinaturaPublicaConfirmacao", context => RateLimitPartition.GetFixedWindowLimiter(
        GetClientIp(context),
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(10),
            QueueLimit = 0
        }));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ninx API",
        Version = "v1",
        Description = "API de gestão de comércio, estoque e vendas."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite seu token JWT"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }

    c.EnableAnnotations();
    c.OperationFilter<AuthorizeOperationFilter>();
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("NinxFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// Precisa vir antes de tudo que lê o IP ou o esquema da requisição.
app.UseForwardedHeaders();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("NinxFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static string GetClientIp(HttpContext context)
{
    return context.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";
}
