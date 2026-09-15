using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ninx.Ioc.Extensions
{
    public static class JwtExtension
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var secret = configuration["Jwt:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException(
                    "Configuração 'Jwt:Secret' ausente. Defina JWT_SECRET no .env ou a variável de ambiente Jwt__Secret.");

            var key = Encoding.UTF8.GetBytes(secret);

            // HMAC-SHA256 exige chave de pelo menos 256 bits; abaixo disso a biblioteca
            // falharia só na emissão do primeiro token, longe da causa real.
            if (key.Length < 32)
                throw new InvalidOperationException(
                    $"'Jwt:Secret' precisa ter pelo menos 32 bytes para HMAC-SHA256 (atual: {key.Length}).");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}