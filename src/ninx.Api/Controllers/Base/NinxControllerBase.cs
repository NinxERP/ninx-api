using Microsoft.AspNetCore.Mvc;
using ninx.Domain.Enums;
using System.Security.Claims;

namespace ninx.Api.Controllers
{
    public class NinxControllerBase : ControllerBase
    {
        protected int GetUsuarioId() => int.Parse(User.FindFirstValue("usuarioId")!);
        protected int GetComercioId() => int.Parse(User.FindFirstValue("comercioId")!);
        protected Permissao GetPermissao() => Enum.Parse<Permissao>(User.FindFirstValue("permissao")!);
    }
}
