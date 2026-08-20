using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ninx.Api.Controllers
{
    public class NinxControllerBase : ControllerBase
    {
        protected int GetUsuarioId() => int.Parse(User.FindFirstValue("usuarioId")!);
        protected int GetComercioId() => int.Parse(User.FindFirstValue("comercioId")!);
        protected int GetCargoId() => int.Parse(User.FindFirstValue("cargoId")!);
        protected int GetCargoPeso() => int.Parse(User.FindFirstValue("cargoPeso")!);
        protected bool GetAdmin() => bool.Parse(User.FindFirstValue("admin")!);
    }
}
