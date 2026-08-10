using ninx.Communication;

namespace ninx.Application.Services
{
    public interface IRelatorioService
    {
        Task<RelatorioDashboardResponse> GetDashboardAsync(int comercioId, RelatorioDashboardRequest request);
        Task<List<ProdutoCurvaAbcResponse>> GetCurvaAbcAsync(int comercioId, RelatorioDashboardRequest request);
        Task<RelatorioMargemResponse> GetMargemAsync(int comercioId, RelatorioDashboardRequest request);
        Task<RelatorioAgingResponse> GetAgingRecebiveisAsync(int comercioId);
        Task<List<VendedorDesempenhoResumo>> GetDesempenhoVendedoresAsync(int comercioId, RelatorioDashboardRequest request);
        Task<RelatorioPicoVendasResponse> GetPicoVendasAsync(int comercioId, RelatorioDashboardRequest request);
        Task<List<ClienteInativoResumo>> GetClientesInativosAsync(int comercioId, RelatorioClientesInativosRequest request);
        Task<List<ClienteLimiteCreditoResumo>> GetUsoLimiteCreditoAsync(int comercioId);
        Task<RelatorioGiroEstoqueResponse> GetGiroEstoqueAsync(int comercioId, RelatorioDashboardRequest request);
        Task<List<ProdutoVencendoResumo>> GetProdutosVencendoAsync(int comercioId, RelatorioProdutosVencendoRequest request);
        Task<RelatorioComparativoComerciosResponse> GetComparativoComerciosAsync(int usuarioId, RelatorioDashboardRequest request);
    }
}
