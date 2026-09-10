using ninx.Communication;

namespace ninx.Domain.Interfaces
{
    public interface IRelatorioRepository
    {
        Task<FaturamentoResumo> GetFaturamentoResumoAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<FormaPagamentoResumo>> GetFormasPagamentoResumoAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<FiadoResumo> GetFiadoResumoAsync(int comercioId);
        Task<List<ProdutoVendidoResumo>> GetProdutosMaisVendidosAsync(int comercioId, DateTime inicio, DateTime fim, int quantidade);
        Task<List<ProdutoVendidoResumo>> GetProdutosMenosVendidosAsync(int comercioId, DateTime inicio, DateTime fim, int quantidade);
        Task<List<EstoqueBaixoResumo>> GetEstoqueBaixoAsync(int comercioId, int quantidade);
        Task<CancelamentoResumo> GetCancelamentosResumoAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<ProdutoVendidoResumo>> GetProdutosVendidosAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<ProdutoMargemResumo>> GetMargemPorProdutoAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<CategoriaMargemResumo>> GetMargemPorCategoriaAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<int> GetQuantidadeProdutosVendidosSemCustoAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<VendaEmAbertoResumo>> GetVendasFiadoEmAbertoAsync(int comercioId);
        Task<List<VendedorDesempenhoResumo>> GetDesempenhoVendedoresAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<PicoPorHoraResumo>> GetPicoPorHoraAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<PicoPorDiaSemanaResumo>> GetPicoPorDiaSemanaAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<ClienteInativoResumo>> GetClientesInativosAsync(int comercioId, int diasSemComprar);
        Task<List<ClienteLimiteCreditoResumo>> GetUsoLimiteCreditoAsync(int comercioId);
        Task<List<GiroEstoqueResumo>> GetGiroEstoqueAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<ProdutoParadoResumo>> GetProdutosParadosAsync(int comercioId, DateTime inicio, DateTime fim);
        Task<List<ProdutoVencendoResumo>> GetProdutosVencendoAsync(int comercioId, int diasLimite);
        Task<List<ComercioComparativoResumo>> GetComparativoComerciosAsync(List<int> comercioIds, DateTime inicio, DateTime fim);
    }
}
