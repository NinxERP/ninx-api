using ninx.Domain.Entities;

namespace ninx.Domain.Interfaces
{
    public interface ITokenProvider
    {
        public string GerarToken(Usuario usuario, int comercioIdSelecionado, Cargo cargoNoComercio, string nomeComercio);
    }
}
