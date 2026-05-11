using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings.CategoriaProduto
{
    public class CategoriaProdutoMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<CategoriaProduto, CategoriaProdutoResponse>();
        }
    }
}
