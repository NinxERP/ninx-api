using Mapster;
using ninx.Communication;
using ninx.Domain.Entities;

public class ProdutoMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        TypeAdapterConfig<Produto, ProdutoResponse>
            .NewConfig()
            .Map(dest => dest.CategoriaNome, src => src.Categoria != null ? src.Categoria.Nome : null)
            .Map(dest => dest.EstoqueID, src => src.Estoque != null ? src.Estoque.EstoqueID : (int?)null)
            .Map(dest => dest.Quantidade, src => src.Estoque != null ? src.Estoque.Quantidade : (decimal?)null)
            .Map(dest => dest.QuantidadeMinima, src => src.Estoque != null ? src.Estoque.QuantidadeMinima : (decimal?)null);
    }
}