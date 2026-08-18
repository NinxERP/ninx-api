using ninx.Domain.Entities;

namespace ninx.Application.Services
{
    public interface IAutorizacaoGlobalService
    {
        Task<Usuario> GarantirAdministradorGlobalAsync(int usuarioIdLogado);
    }
}
