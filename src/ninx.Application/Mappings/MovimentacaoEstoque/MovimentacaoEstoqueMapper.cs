using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings.MovimentacaoEstoque
{
    public class MovimentacaoEstoqueMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<MovimentacaoEstoque, MovimentacaoEstoqueResponse>();
        }
    }
}
