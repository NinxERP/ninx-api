using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using ninx.Domain.Entities;
using ninx.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ninx.Infra
{
    public class TokenProvider : ITokenProvider
    {
        private readonly IConfiguration _configuration;

        public TokenProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(Usuario usuario, int comercioIdSelecionado, Cargo cargoNoComercio, string nomeComercio)
        {
            var claims = new List<Claim>
            {
                new Claim("usuarioId", usuario.UsuarioID.ToString()),
                new Claim("nome", usuario.Nome),
                new Claim("email", usuario.Email),
                new Claim("comercioId", comercioIdSelecionado.ToString()),
                new Claim("cargoId", cargoNoComercio.CargoID.ToString()),
                new Claim("cargoNome", cargoNoComercio.Nome),
                new Claim("cargoEhProprietario", cargoNoComercio.EhProprietario.ToString()),
                new Claim("cargoPermissoes", string.Join(',', cargoNoComercio.CargoPermissoes.Select(cp => cp.Permissao.Chave))),
                new Claim("nomeComercio", nomeComercio),
                new Claim("admin", usuario.Admin.ToString())
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}