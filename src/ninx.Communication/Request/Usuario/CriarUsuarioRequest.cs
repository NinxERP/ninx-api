// ninx.Communication/Request/Usuario/CriarUsuarioRequest.cs
namespace ninx.Communication.Request.Usuario
{
    public class CriarUsuarioRequest
    {
        public string Nome { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Senha { get; set; } = null!;
        public string Permissao { get; set; } = null!;
    }
}