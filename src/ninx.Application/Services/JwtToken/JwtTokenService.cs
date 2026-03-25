using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ninx.Application.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(Usuario usuario, int? comercioIdSelecionado)
        {
            var claims = new[]
            {
                new Claim("usuarioId", usuario.UsuarioID.ToString()),
                new Claim("nome", usuario.Nome),
                new Claim("permissao", usuario.Permissao.ToString()),
                new Claim("comercioId", comercioIdSelecionado?.ToString() ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])), signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}