
using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces.Services
{
    public interface IJwtTokenService
    {
        public string GerarToken(Usuario usuario, int? comercioIdSelecionado);
    }
}
