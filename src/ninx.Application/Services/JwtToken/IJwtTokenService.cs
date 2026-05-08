using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Application.Services
{
    public interface IJwtTokenService
    {
        public string GerarToken(Usuario usuario, int comercioIdSelecionado, Permissao permissaoNoComercio);
    }
}
