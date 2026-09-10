using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi;
using ninx.Api.Filters;
using ninx.Api.Middlewares;
using ninx.Ioc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationActionFilter>();
});
builder.Services.AddInfrastructure(builder.Configuration);

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

var origensPermitidas = new[]
{
    "http://localhost:5173",
    "http://localhost:1420",
    "tauri://localhost",
    "http://tauri.localhost",
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("NinxFrontend", policy =>
    {
        policy.WithOrigins(origensPermitidas)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
