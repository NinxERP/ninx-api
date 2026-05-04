using Mapster;
using ninx.Communication.Response;
using ninx.Domain.Entities;

namespace ninx.Application.Mappings
{
    public class AssinaturaEletronicaMapper : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AssinaturaEletronica, AssinaturaEletronicaResponse>()
            .Map(dest => dest.NomeCliente, src => src.Venda.Cliente.Nome)
            .Map(dest => dest.NomeComercio, src => src.Venda.Comercio.NomeComercio)
            .Map(dest => dest.ValorTotal, src => src.Venda.Total)
            .Map(dest => dest.DataVenda, src => src.Venda.CriadoEm)
            .Map(dest => dest.Itens, src => src.Venda.ItensVenda);
        }
    }
}
