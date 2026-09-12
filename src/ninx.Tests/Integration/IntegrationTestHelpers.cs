using Microsoft.Extensions.DependencyInjection;
using ninx.Data.Context;
using ninx.Domain.Entities;
using ninx.Domain.Enums;
using ninx.Domain.Interfaces;

namespace ninx.Tests.Integration
{
    /// <summary>
    /// Helpers de seed de dados e emissão de token JWT reais (via ITokenProvider registrado
    /// no container de DI da factory), usados pelos testes de integração.
    /// </summary>
    public static class IntegrationTestHelpers
    {
        public static (Usuario Usuario, Cargo Cargo, Comercio Comercio, UsuarioComercio Vinculo) SeedComercioComUsuario(
            NinxDB db,
            string emailUsuario = "dono@teste.com",
            string nomeComercio = "Comércio Teste",
            bool ehProprietario = true,
            bool admin = false)
        {
            var comercio = new Comercio { NomeComercio = nomeComercio, Ativo = true };
            db.Comercio.Add(comercio);
            db.SaveChanges();

            var cargo = new Cargo { Nome = "Dono", EhProprietario = ehProprietario, ComercioID = comercio.ComercioID, Ativo = true };
            db.Cargos.Add(cargo);
            db.SaveChanges();

            var usuario = new Usuario
            {
                Nome = "Usuário Teste",
                Email = emailUsuario,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Senha123!"),
                Ativo = true,
                Admin = admin
            };
            db.Usuarios.Add(usuario);
            db.SaveChanges();

            var vinculo = new UsuarioComercio
            {
                UsuarioID = usuario.UsuarioID,
                ComercioID = comercio.ComercioID,
                CargoID = cargo.CargoID,
                Ativo = true
            };
            db.UsuarioComercio.Add(vinculo);

            db.AssinaturaPlano.Add(new AssinaturaPlano
            {
                ComercioID = comercio.ComercioID,
                Plano = PlanoAssinatura.Mensal,
                DataInicio = DateTime.UtcNow.AddDays(-1),
                DataFim = DateTime.UtcNow.AddMonths(1),
                Status = StatusAssinatura.Ativa
            });

            db.SaveChanges();

            return (usuario, cargo, comercio, vinculo);
        }

        public static string GerarTokenPara(NinxWebApplicationFactory factory, Usuario usuario, Cargo cargo, int comercioId, string nomeComercio)
        {
            using var scope = factory.Services.CreateScope();
            var tokenProvider = scope.ServiceProvider.GetRequiredService<ITokenProvider>();
            return tokenProvider.GerarToken(usuario, comercioId, cargo, nomeComercio);
        }
    }
}
