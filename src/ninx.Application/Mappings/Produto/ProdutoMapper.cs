using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;

public class ProdutoMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Produto, ProdutoResponse>()
            .Map(dest => dest.CategoriaNome, src => src.Categoria != null ? src.Categoria.Nome : null);
    }
}