using ninx.Domain.Entities;
using ninx.Domain.Enums;

namespace ninx.Domain
{
    public interface ITokenProvider
    {
        public string GerarToken(Usuario usuario, int comercioIdSelecionado, Permissao permissaoNoComercio);
    }
}
